using System;
using System.Collections.Generic;
using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class VFSFile : IIndexNode
    {
        public VFSFile(string name, string source)
            : this(name, File.ReadAllBytes(source))
        {
        }

        private VFSFile(string name, byte[] data)
        {
            Name = PathParser.NormalizeName(name);
            if (Name.Length <= 0) throw new ArgumentException("Name must not be empty!");

            Data = data;
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public byte[] Data { get; private set; }
    }
}
