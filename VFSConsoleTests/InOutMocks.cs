using System;
using System.IO;

namespace VFSConsoleTests
{
    public class InOutMocks : IDisposable
    {
        private readonly MemoryStream _memoryIn;
        private readonly MemoryStream _memoryOut;
        private readonly StreamWriter _inWriter;
        private readonly StreamReader _outReader;

        public StreamWriter Out { get; private set; }

        public StreamReader In { get; private set; }

        public InOutMocks()
        {
            _memoryIn = new MemoryStream();
            _inWriter = new StreamWriter(_memoryIn);
            In = new StreamReader(_memoryIn);

            _memoryOut = new MemoryStream();
            _outReader = new StreamReader(_memoryOut);
            Out = new StreamWriter(_memoryOut);
        }

        public void FakeInLine(string line, bool seekToBeginning = false)
        {
            _inWriter.WriteLine(line);
            _inWriter.Flush();
            if (seekToBeginning) _inWriter.BaseStream.Position = 0;
        }

        public void Dispose()
        {
            In.Dispose();
            _inWriter.Dispose();
            Out.Dispose();
            _outReader.Dispose();
            _memoryIn.Dispose();
            _memoryOut.Dispose();
        }

        public string FakeOutLine(bool seekToBeginning = false)
        {
            Out.Flush();
            if (seekToBeginning) _outReader.BaseStream.Position = 0;
            return _outReader.ReadLine();
        }
    }
}