using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    public class Folder : IComparable, IIndexNode
    {
        public Folder(string name)
            : this()
        {
            Name = name;
        }

        public Folder()
        {
            IndexNodes = new List<IIndexNode>();
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public IList<IIndexNode> IndexNodes { get; set; }

        public int CompareTo(object obj)
        {
            var node = obj as IIndexNode;
            if (node == null) return -1;
            return String.Compare(Name, node.Name, StringComparison.Ordinal);
        }

    }
}