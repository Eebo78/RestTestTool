using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Epim.RestClient;
using Epim.RestTest.Helpers;
using Epim.RestTest.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace Epim.RestTest.ViewModel
{
    public class PerformanceTestViewModel : ViewModelBase
    {
        private int _concurrentClients;
        private ObservableCollection<Organization> _organizations;
        private Organization _selectedOrganization;
        private ObservableCollection<ElhResponse> _statistics;
        private int _noOfRequests;
        private HttpVerb _selectedMethod;
        private string _endPoint;
        private string _csvFilename;
        private bool _canDumpToCsv;
        private readonly XmlHelper _xmlHelper = new XmlHelper();
        private int _totalContainers;
        private bool _stopTestRun;
        private string _customCsvFilename;
        private bool _canStopTests;
        private bool _canStartTests = true;

        #region Constructor

        public PerformanceTestViewModel()
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;  //Allow 1000 concurrent connections
            
            InitCommands();
            Statistics = new ObservableCollection<ElhResponse>();
            Organizations = new ObservableCollection<Organization>();
            GetOrganizations();
            ConcurrentClients = 10;
            NoOfRequests = 20;
            SelectedMethod = HttpVerb.GET;
            EndPoint = "https://www.logisticshub.no/elh/ccus";
            CustomCsvFilename = "MyDump.csv";

        }
        #endregion

        #region Commands

        private void InitCommands()
        {
            StartTestCommand = new RelayCommand(() =>CreateConcurrentTasks(), () => _canStartTests);
            StopTestsCommand = new RelayCommand(() => _stopTestRun = true, () => _canStopTests);
            WriteToCsvCommand = new RelayCommand(() =>
            {
                _csvFilename = CustomCsvFilename;
                WriteToCsv();
            }, () =>_canDumpToCsv);
        }

        public RelayCommand StartTestCommand { get; private set; }
        public RelayCommand StopTestsCommand { get; private set; }
        public RelayCommand WriteToCsvCommand { get; private set; } 
        #endregion

        #region Private Methods

        private void WriteToCsv()
        {
            _canStartTests = false;
            _canStopTests = false;
            _canDumpToCsv = false;
            StartTestCommand.RaiseCanExecuteChanged();
            StopTestsCommand.RaiseCanExecuteChanged();
            WriteToCsvCommand.RaiseCanExecuteChanged();

            //Write CSV
            //before your loop
            var csv = new StringBuilder();
            var csvDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\CSV";

            if (!Directory.Exists(csvDirectory))
                Directory.CreateDirectory(csvDirectory);

            //Create the CSV folder
            var filePath = string.Format("{0}\\{1}", csvDirectory, _csvFilename);
            
            var newLine = "ClientID;StartTime;ResponseTime"+ Environment.NewLine;
            csv.Append(newLine);
            foreach (var response in Statistics.OrderBy(c => c.Client).ThenBy(c => c.StartOfRequest))
            {
                newLine = string.Format("{0};{1};{2}{3}", response.Client, response.StartOfRequest, response.Time, Environment.NewLine);
                csv.Append(newLine);    
            }
            
            //after your loop
            File.WriteAllText(filePath, csv.ToString());

            _canStartTests = true;
            _canDumpToCsv = true;
            StartTestCommand.RaiseCanExecuteChanged();
            WriteToCsvCommand.RaiseCanExecuteChanged();
        }
        
        private async Task CreateConcurrentTasks()
        {
            Statistics.Clear();
            _stopTestRun = false;
            _canStartTests = false;
            _canStopTests = true;
            _canDumpToCsv = false;
            StartTestCommand.RaiseCanExecuteChanged();
            StopTestsCommand.RaiseCanExecuteChanged();
            WriteToCsvCommand.RaiseCanExecuteChanged();
            var requests = new List<Task>();
            
            //Create number of clients
            //for (var i = 0; i < NoOfRequests; i++)
            //{
                //Add all requests to a list
            for (var i = 0; i < NoOfRequests; i++)
            {
                if (_stopTestRun) break;
                for (var j = 0; j < ConcurrentClients; j++)
                {
                    requests.Add(MakeAsyncRequest(j));
                }
                await Task.WhenAll(requests);
            }
            _canStartTests = true;
            _canStopTests = false;
            _canDumpToCsv = Statistics.Any();

            _csvFilename = string.Format("ConcurrentClients_{0}_Request_{1}_{2}.csv",ConcurrentClients, NoOfRequests, SelectedOrganization.Name);
            WriteToCsv();

            StartTestCommand.RaiseCanExecuteChanged();
            StopTestsCommand.RaiseCanExecuteChanged();
            WriteToCsvCommand.RaiseCanExecuteChanged();
            //Wait for all requests to finish
            //while (requests.Any() && ! _stopTestRun)
            //{
            //    // Identify the first task that completes.
            //    var firstFinishedTask = await Task.WhenAny(requests);

            //    // ***Remove the selected task from the list so that you don't 
            //    // process it more than once.
            //    requests.Remove(firstFinishedTask);

            //    // Await the completed task. 
            //    var response = await firstFinishedTask;
            //    Statistics.Add(response);
            //}
            //}
            //Dump result to csv
        }

        //private async Task<ElhResponse> GetContainersAsync(int taskNo)
        private async Task MakeAsyncRequest(int taskNo)
        {
            System.Diagnostics.Debug.WriteLine("Creating new client @:" + DateTime.Now);
            var client = new Client(SelectedOrganization.Certificate)
            {
                EndPoint = EndPoint,
                Method = SelectedMethod,
                ClientId = string.Format("Client_{0}", (taskNo + 1).ToString("000"))
            };

            var response = await client.MakeRequest();
            //response.RequestNo = i + 1;
            Statistics.Add(response);
        }
        
        private async Task GetContainerCountAsync()
        {
            var client = new Client(SelectedOrganization.Certificate)
            {
                Method = HttpVerb.GET
            };

            var checkForMore = true;
            //Speed up the epim request
            var page = 0;
            TotalContainers = 0;
            if (SelectedOrganization.Name.ToLower().Equals("epim"))
            {
                TotalContainers = 14000;
                page = 28;
            }
            while(checkForMore)
            {
                page++;
                client.EndPoint = string.Format("https://www.logisticshub.no/elh/ccus?page-size=500&page={0}", page);
                
                var ccus = await client.MakeRequest();

                var containers = _xmlHelper.Deserialize<ContainerCollection>(ccus.Result);

                TotalContainers += containers.Containers.Count();

                if (containers.Containers.Count < 500)
                {
                    checkForMore = false;
                }
            }
        }

        private void GetOrganizations()
        {
            var store = new X509Store(StoreLocation.CurrentUser);

            store.Open(OpenFlags.ReadOnly);

            Organizations = new ObservableCollection<Organization>();
            foreach (var certificate in store.Certificates)
            {
                long orgNo;
                long gln;

                GetOranizationNoAndGln(certificate.FriendlyName, out orgNo, out gln);

                Organizations.Add(new Organization
                {
                    Name = certificate.FriendlyName,
                    Certificate = certificate,
                    Gln = gln,
                    OrgNo = orgNo
                });
            }

            store.Close();

            SelectedOrganization = Organizations.SingleOrDefault(c => c.Certificate.FriendlyName.ToLower().Equals("epim"));
        }

        private static void GetOranizationNoAndGln(string name, out long orgNo, out long gln)
        {
            orgNo = -1;
            gln = -1;
            switch (name.ToLower())
            {
                case "epim":
                    {
                        orgNo = 992100400;
                        gln = 7080001336059;
                        return;
                    }
                case "norsea":
                    {
                        orgNo = 982259290;
                        gln = 7080003551528;
                        return;
                    }
            }
        }
        #endregion
        
        #region Properties

        public int ConcurrentClients
        {
            get { return _concurrentClients; }
            set
            {
                _concurrentClients = value;
                RaisePropertyChanged(() => ConcurrentClients);
            }
        }

        public string CustomCsvFilename
        {
            get { return _customCsvFilename; }
            set
            {
                _customCsvFilename = value; 
                RaisePropertyChanged(() => CustomCsvFilename);
            }
        }

        public string EndPoint
        {
            get { return _endPoint; }
            set
            {
                _endPoint = value;
                RaisePropertyChanged(() => EndPoint);
            }
        }

        public IEnumerable<HttpVerb> Methods
        {
            get
            {
                return Enum.GetValues(typeof(HttpVerb)).Cast<HttpVerb>();
            }
        }

        public int NoOfRequests
        {
            get { return _noOfRequests; }
            set
            {
                _noOfRequests = value;
                RaisePropertyChanged(() => NoOfRequests);
            }
        }

        public ObservableCollection<Organization> Organizations
        {
            get { return _organizations; }
            set
            {
                _organizations = value;
                RaisePropertyChanged(() => Organizations);
            }
        }

        public HttpVerb SelectedMethod  
        {
            get { return _selectedMethod; }
            set
            {
                _selectedMethod = value;
                RaisePropertyChanged(() => SelectedMethod);
            }
        }

        public Organization SelectedOrganization
        {
            get { return _selectedOrganization; }
            set
            {
                _selectedOrganization = value;
                RaisePropertyChanged(() => SelectedOrganization);
                GetContainerCountAsync();
            }
        }

        public ObservableCollection<ElhResponse> Statistics 
        {
            get { return _statistics; }
            set
            {
                _statistics = value;
                RaisePropertyChanged(() => Statistics);
            }
        }
        
        public int TotalContainers  
        {
            get { return _totalContainers; }
            set
            {
                _totalContainers = value; 
                RaisePropertyChanged(() => TotalContainers);
            }
        }

        #endregion
    }
}
