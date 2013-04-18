using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using VFSBase;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var container = new UnityContainer().LoadConfiguration();
                using (var c = container.Resolve<ConsoleApplication>())
                {
                    c.Run();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine("Exit with any input...");
                Console.ReadLine();
            }
        }
    }
}
