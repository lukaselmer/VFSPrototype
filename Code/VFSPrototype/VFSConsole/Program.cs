using System;
using System.Collections;
using System.IO;
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
            const string file = "./vfs.xxx";
            if (File.Exists(file)) File.Delete(file);

            var container = new UnityContainer();
            container.RegisterType<IFileSystemManipulator, FileSystemManipulator>();
            container.RegisterInstance(new FileSystemOptions(file, 1024 * 1024 * 1024));
            container.RegisterInstance(new ConsoleApplicationSettings(Console.In, Console.Out));
            var c = container.Resolve<ConsoleApplication>();
            c.Run();
        }
    }
}
