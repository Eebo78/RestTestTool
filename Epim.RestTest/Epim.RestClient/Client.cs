using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Epim.RestClient
{
    public class Client
    {
        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public string ContentType { get; set; }
        public string ClientId { get; set; }
        public string PostData { get; set; }
        public X509Certificate2 Certificate { get; set; }


        private int _requestNo;
        public Client(X509Certificate2 certificate)
        {
            EndPoint = "";
            Method = HttpVerb.GET;
            ContentType = "application/xml";
            PostData = "";
            Certificate = certificate;
        }
        //public RestClient(string endpoint)
        //{
        //    EndPoint = endpoint;
        //    Method = HttpVerb.GET;
        //    ContentType = "text/xml";
        //    PostData = "";
        //}

        //public RestClient(string endpoint, HttpVerb method, X509Certificate2 certificate)
        //{
        //    EndPoint = endpoint;
        //    Method = method;
        //    ContentType = "text/xml";
        //    PostData = "";
        //    Certificate = certificate;
        //}

        //public RestClient(string endpoint, HttpVerb method, string postData)
        //{
        //    EndPoint = endpoint;
        //    Method = method;
        //    ContentType = "text/xml";
        //    PostData = postData;
        //}
        

        //public string MakeRequest()
        public async Task<ElhResponse> MakeRequest()
        {
            return await MakeRequest("");
        }

        //public async Task<string> MakeRequest(string parameters)
        public async Task<ElhResponse> MakeRequest(string parameters)
        {
            var request = (HttpWebRequest)WebRequest.Create(EndPoint + parameters);

            
            request.Method = Method.ToString();
            request.ContentLength = 0;
            request.ContentType = ContentType;
            request.ClientCertificates.Add(Certificate);

            if (!string.IsNullOrEmpty(PostData) && (Method == HttpVerb.PUT || Method == HttpVerb.POST))
            {
                var encoding = new UTF8Encoding();
                var bytes = Encoding.GetEncoding("iso-8859-1").GetBytes(PostData);
                request.ContentLength = bytes.Length;

                using (var writeStream = request.GetRequestStream())
                {
                    writeStream.Write(bytes, 0, bytes.Length);
                }
            }

            _requestNo ++;
            var startTime = DateTime.Now;
            try
            {
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    var endTime = DateTime.Now;

                    var responseValue = string.Empty;
                    
                    // grab the response
                    using (var responseStream = response.GetResponseStream())
                    {
                        if (responseStream != null)
                            using (var reader = new StreamReader(responseStream))
                            {
                                responseValue = reader.ReadToEnd();
                            }
                    }

                    return new ElhResponse
                    {
                        EndPoint = EndPoint,
                        Method = Method,
                        StatusCode = response.StatusCode.ToString(),
                        Time = (int) Math.Ceiling((endTime - startTime).TotalMilliseconds),
                        Result = responseValue,
                        StartOfRequest = startTime,
                        EndOfRequest = endTime,
                        Client = ClientId,
                        RequestNo = _requestNo
                    };
                }
            }
            catch (WebException wex)
            {
                var endTime = DateTime.Now;
                if (wex.Status == WebExceptionStatus.ProtocolError)
                {
                    var response = wex.Response as HttpWebResponse;
                    if (response != null)
                    {
                        return new ElhResponse
                        {
                            EndPoint = EndPoint,
                            Method = Method, 
                            StatusCode =  response.StatusCode.ToString(), 
                            StartOfRequest = startTime,
                            Time = (int)Math.Ceiling((endTime - startTime).TotalMilliseconds),
                            Result = wex.Message,
                            Client = ClientId,
                            RequestNo = _requestNo
                        };
                    }
                }
                return new ElhResponse
                {
                    EndPoint = EndPoint,
                    StatusCode = "Error",
                    
                    Method = Method, 
                    StartOfRequest = startTime,
                    Time = (int)Math.Ceiling((endTime - startTime).TotalMilliseconds),
                    Result = "Unknown error: " + wex.Message,
                    Client = ClientId,
                    RequestNo = _requestNo
                };
            }
            catch (Exception ex)
            {
                
                return new ElhResponse
                {
                    EndPoint = EndPoint,
                    Method = Method,
                    StatusCode = "Error",
                    
                    StartOfRequest = startTime,
                    Time = (int)Math.Ceiling((DateTime.Now - startTime).TotalMilliseconds),
                    Result = "Unknown error: " + ex.Message
                };
            }
        }
    }
}
