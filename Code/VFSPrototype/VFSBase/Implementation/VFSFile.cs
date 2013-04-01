using System;
using System.Collections.Generic;
using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class VFSFile : IComparable, IIndexNode
    {
        public VFSFile(string name, string source)
            : this(name, File.ReadAllBytes(source))
        {
        }

        private VFSFile(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public IList<IIndexNode> IndexNodes { get; set; }
        public byte[] Data { get; private set; }

        public int CompareTo(object obj)
        {
            var file = obj as VFSFile;
            if (file == null) return -1;
            return String.Compare(Name, file.Name, StringComparison.Ordinal);
        }
    }
}
