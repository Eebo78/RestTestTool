using System;
using System.Net;

namespace Epim.RestClient
{
    public class ElhResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string EndPoint { get; set; }
        public HttpVerb Method { get; set; }
        public int Time { get; set; }
        public string Result { get; set; }
        public DateTime StartOfRequest { get; set; }
        public DateTime EndOfRequest { get; set; }
        
    }
}
