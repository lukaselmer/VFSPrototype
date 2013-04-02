using System;
using System.Collections.Generic;
using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class VFSFile : IComparable, IIndexNode
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

        public byte[] Data { get; private set; }

        public int CompareTo(object obj)
        {
            var node = obj as IIndexNode;
            if (node == null) return -1;
            return String.Compare(Name, node.Name, StringComparison.Ordinal);
        }
    }
}
