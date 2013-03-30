using System.IO;

namespace VFSConsole
{
    public class ConsoleApplicationSettings
    {
        private readonly TextReader _textReader;
        private readonly TextWriter _textWriter;

        public ConsoleApplicationSettings(TextReader textReader, TextWriter textWriter)
        {
            _textReader = textReader;
            _textWriter = textWriter;
        }

        public TextReader TextReader
        {
            get { return _textReader; }
        }

        public TextWriter TextWriter
        {
            get { return _textWriter; }
        }
    }
}