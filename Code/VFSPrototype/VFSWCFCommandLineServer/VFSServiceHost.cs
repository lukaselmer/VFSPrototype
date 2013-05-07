using System.ServiceModel;
using VFSWCFService.DiskService;

namespace VFSWCFCommandLineServer
{
    internal class VFSServiceHost
    {
        private static ServiceHost _serviceHost;

        public static void StartService()
        {
            //Instantiate new ServiceHost 
            _serviceHost = new ServiceHost(typeof(DiskService));

            //Open myServiceHost
            _serviceHost.Open();
        }

        public static void StopService()
        {
            //Call StopService from your shutdown logic (i.e. dispose method)
            if (_serviceHost.State != CommunicationState.Closed) _serviceHost.Close();
        }
    }
}