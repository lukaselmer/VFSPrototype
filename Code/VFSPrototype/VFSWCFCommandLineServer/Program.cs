using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;

namespace VFSWCFCommandLineServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                Console.WriteLine("VFSServiceHost starting...");
                VFSServiceHost.StartService();
                Console.WriteLine("VFSServiceHost started...");
                Console.ReadLine();
                VFSServiceHost.StopService();
            }
            catch (AddressAccessDeniedException ex)
            {
                Console.WriteLine("An exception occurred: {0}", ex.Message);
                Console.WriteLine("Hint: http://stackoverflow.com/questions/885744/wcf-servicehost-access-rights");
                Console.WriteLine("netsh http add urlacl url=http://+:8033/DiskService/ user=mylocaluser");
                Console.WriteLine("e.g. netsh http add urlacl url=http://+:8033/DiskService/ user=\"lukas.elmer@gmail.com\"");
                Console.ReadLine();
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine("An exception occurred: {0}", ex.Message);
                Console.ReadLine();
            }
        }
    }
}
