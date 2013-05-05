using VFSBase.Interfaces;

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

        public Folder()
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

        public long PredecessorBlockNr { get; set; }

        internal bool IsRoot { get; set; }

        public long Version { get; set; }

        /// <summary>
        /// Gets or sets the blocks used. This is needed for the root folder.
        /// </summary>
        /// <value>
        /// The blocks used.
        /// </value>
        public long BlocksUsed { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Folder)obj);
        }

        protected bool Equals(Folder other)
        {
            return BlockNumber == other.BlockNumber;
        }

        public override int GetHashCode()
        {
            return BlockNumber.GetHashCode();
        }

    }
}
