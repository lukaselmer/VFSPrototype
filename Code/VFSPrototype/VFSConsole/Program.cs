using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;
using VFSBase;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer().LoadConfiguration();
            var c = container.Resolve<ConsoleApplication>();
            c.Run();
        }
    }
}
