using System.IO;

namespace VFSConsole
{
    public class ConsoleApplicationSettings : IConsoleApplicationSettings
    {
        private readonly TextReader _reader;
        private readonly TextWriter _writer;

        public ConsoleApplicationSettings(TextReader reader, TextWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public TextReader Reader
        {
            get { return _reader; }
        }

        public TextWriter Writer
        {
            get { return _writer; }
        }
    }
}
