using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using VFSBase;

namespace VFSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            // TODO: use unity to resolve the file system
            var fileSystem = new FileSystem("./vfs", 1024 * 1024 * 1024);
            var c = new ConsoleApplication(Console.In, Console.Out, new FileSystemManipulator(fileSystem));
            c.Run();
        }
    }
}
