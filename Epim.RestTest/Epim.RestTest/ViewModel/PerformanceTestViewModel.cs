using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Epim.RestClient;
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

        #region Constructor

        public PerformanceTestViewModel()
        {
            InitCommands();
            Statistics = new ObservableCollection<ElhResponse>();
            Organizations = new ObservableCollection<Organization>();
            GetOrganizations();
        }
        #endregion

        #region Commands

        private void InitCommands()
        {
            StartTestCommand = new RelayCommand(() => GetContainers());
        }

        public RelayCommand StartTestCommand { get; private set; }
        #endregion

        #region Private Methods
        private async Task GetContainers()
        {
            var requests = new List<Task<ElhResponse>>();

            //Configure MAximum allowed concurrent clients
            ServicePointManager.DefaultConnectionLimit = ConcurrentClients; 

            //Create number of clients
            for (var i = 0; i < ConcurrentClients; i++)
            {
                requests.Add(GetContainersAsync(i));
            }
            var startTime = DateTime.Now;
            while (requests.Any())
            {
                // Identify the first task that completes.
                var firstFinishedTask = await Task.WhenAny(requests);

                // ***Remove the selected task from the list so that you don't 
                // process it more than once.
                requests.Remove(firstFinishedTask);

                // Await the completed task. 
                var response = await firstFinishedTask;
                Statistics.Add(response);
            }
        }

        async Task<ElhResponse> GetContainersAsync(int taskNo)
        {
            //await Task.Delay(taskNo*500);
            System.Diagnostics.Debug.WriteLine("Creating new client @:" + DateTime.Now);
            var client = new Client(SelectedOrganization.Certificate)
            {
                EndPoint = "https://www.logisticshub.no/elh/ccus"
            };

            return await client.MakeRequest();
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

        public ObservableCollection<Organization> Organizations
        {
            get { return _organizations; }
            set
            {
                _organizations = value;
                RaisePropertyChanged(() => Organizations);
            }
        }
        
        public Organization SelectedOrganization
        {
            get { return _selectedOrganization; }
            set
            {
                _selectedOrganization = value;
                RaisePropertyChanged(() => SelectedOrganization);
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

        #endregion
    }
}
