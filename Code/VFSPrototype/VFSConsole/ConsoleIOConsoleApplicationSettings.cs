using System;
using System.IO;

namespace VFSConsole
{
    public class ConsoleIOConsoleApplicationSettings : IConsoleApplicationSettings
    {
        public TextReader Reader
        {
            get { return Console.In; }
        }

        public TextWriter Writer
        {
            get { return Console.Out; }
        }
    }
}