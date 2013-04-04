using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Persistance.Blocks
{
    /// <summary>
    /// Class EmptyBlock
    /// Null object
    /// </summary>
    internal class EmptyBlock : IIndexNode
    {
        private static readonly EmptyBlock TheEmptyBlock = new EmptyBlock();

        public static EmptyBlock Get()
        {
            return TheEmptyBlock;
        }

        private EmptyBlock() { }
        public string Name { get { return ""; } set { } }
        public Folder Parent { get { return null; } set { } }

        public long BlockNumber
        {
            get { throw new NotFoundException(); }
            set { throw new NotFoundException(); }
        }

        public long IndirectNodeNumber
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public long BlocksCount
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }
    }
}
