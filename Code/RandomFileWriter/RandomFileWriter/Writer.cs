using System;
using System.IO;

namespace RandomFileWriter
{
    public class Writer : IDisposable
    {
        private readonly FileStream _file;
        private static readonly int BufferSize = ByteUtil.IntPow(2, 20);
        private readonly long _filesize;

        public Writer(string bigfilePath)
        {
            if (File.Exists(bigfilePath)) throw new ArgumentException(String.Format("File {0} already exists", bigfilePath));
            _file = File.Create(bigfilePath, BufferSize, FileOptions.RandomAccess);
            var b = new byte[BufferSize];
            _file.Write(b, 0, BufferSize);
            _file.Seek(0, SeekOrigin.Begin);
            _filesize = _file.Length;
        }

        public void WriteFile(string path, long offset)
        {
            _file.Seek(offset, SeekOrigin.Begin);
            using (var f = File.OpenRead(path))
            {
                if(_filesize < offset + f.Length) throw new ArgumentException("File is too big to store");

                var bufferSize = BufferSize;
                var buffer = new byte[bufferSize];
                int bytesRead;
                while (0 < (bytesRead = f.Read(buffer, 0, bufferSize))) _file.Write(buffer, 0, bytesRead);
            }
            _file.Seek(0, SeekOrigin.Begin);
        }

        public void Dispose()
        {
            _file.Dispose();
        }

    }
}
