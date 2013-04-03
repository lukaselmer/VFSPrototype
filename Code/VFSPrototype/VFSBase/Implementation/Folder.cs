using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VFSBase.Interfaces;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

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
            Name = "";
            BlocksCount = 0;
        }

        public virtual long BlocksCount { get; set; }

        public string Name { get; set; }

        public virtual long BlockNumber { get; set; }

        public Folder Parent { get; set; }

        public long IndirectNodeNumber { get; set; }
    }
}