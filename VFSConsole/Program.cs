using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;

namespace VFSConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var c = new ConsoleApplication(Console.In, Console.Out);
            c.Run();
        }
    }
}
