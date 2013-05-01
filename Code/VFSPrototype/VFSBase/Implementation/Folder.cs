﻿using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class Folder : IIndexNode
    {
        private long _blocksCount;

        public Folder(string name)
            : this()
        {
            Name = name;
        }

        protected Folder()
        {
            Name = "";
            _blocksCount = 0;
        }

        public string Name { get; set; }

        public virtual long BlockNumber { get; set; }

        public Folder Parent { get; set; }

        public virtual long BlocksCount
        {
            get { return _blocksCount; }
            set { _blocksCount = value; }
        }

        public long IndirectNodeNumber { get; set; }


        public override bool Equals (object obj)
        {
            var fold = obj as Folder;
            if (fold == null)
                return false;

            return this.BlockNumber == fold.BlockNumber &&
                   this.BlocksCount == fold.BlocksCount &&
                   this.IndirectNodeNumber == fold.IndirectNodeNumber &&
                   this.Name == fold.Name;
        }
    }
}
