using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;
using VFSBase.Persistance;

namespace VFSBase.Implementation
{
    internal class Folder : IIndexNode
    {
        public Folder(string name)
            : this()
        {
            Name = name;
        }

        protected Folder()
        {
            IndexNodes = new List<IIndexNode>();
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public IList<IIndexNode> IndexNodes { get; set; }
    }
}