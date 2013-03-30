using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using VFSBase;

namespace VFSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new UnityContainer();
            container.RegisterType<IFileSystemManipulator, FileSystemManipulator>();
            container.RegisterInstance(new FileSystemOptions("./vfs.xxx", 1024 * 1024 * 1024));
            container.RegisterInstance(new ConsoleApplicationSettings(Console.In, Console.Out));
            var c = container.Resolve<ConsoleApplication>();
            c.Run();
        }
    }
}
