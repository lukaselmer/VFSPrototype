using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using VFSWCFService;

namespace VFSWCFServer
{
    /// <summary>
    /// From http://msdn.microsoft.com/en-us/library/ms730935.aspx
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Step 1 Create a URI to serve as the base address.
            var baseAddress = new Uri("http://localhost:8000/");

            // Step 2 Create a ServiceHost instance
            var selfHost = new ServiceHost(typeof(UserService), baseAddress);

            try
            {
                // Step 3 Add a service endpoint.
                selfHost.AddServiceEndpoint(typeof(IUserService), new WSHttpBinding(), "UserService");

                // Step 4 Enable metadata exchange.
                var smb = new ServiceMetadataBehavior { HttpGetEnabled = true };
                selfHost.Description.Behaviors.Add(smb);

                // Step 5 Start the service.
                selfHost.Open();
                Console.WriteLine("The service is ready.");
                Console.WriteLine("Press <ENTER> to terminate service.");
                Console.WriteLine();
                Console.ReadLine();

                // Close the ServiceHostBase to shutdown the service.
                selfHost.Close();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                selfHost.Abort();
                Console.ReadLine();
            }
        }
    }
}
