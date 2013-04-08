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

        public string FakeOutLine(bool seekToBeginning = false)
        {
            Out.Flush();
            if (seekToBeginning) _outReader.BaseStream.Position = 0;
            return _outReader.ReadLine();
        }


        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            // free managed resources
            if (In != null) In.Dispose();
            if (Out != null) Out.Dispose();
            if (_outReader != null) _outReader.Dispose();
            if (_inWriter != null) _inWriter.Dispose();
            if (_memoryIn != null) _memoryIn.Dispose();
            if (_memoryOut != null) _memoryOut.Dispose();
        }
    }
}
