using System.IO;
using VFSBase.Interfaces;

namespace VFSBase.Implementation
{
    internal class VFSFile : IIndexNode
    {
        public VFSFile(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public Folder Parent { get; set; }

        public long BlockNumber { get; set; }
        public long IndirectNodeNumber { get; set; }
        public long BlocksCount { get; set; }

        public long Version { get; set; }

        public int LastBlockSize { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((VFSFile)obj);
        }

        protected bool Equals(VFSFile other)
        {
            return BlockNumber == other.BlockNumber;
        }

        public override int GetHashCode()
        {
            return BlockNumber.GetHashCode();
        }
    }
}
