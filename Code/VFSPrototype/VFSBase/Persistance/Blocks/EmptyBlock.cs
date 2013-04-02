using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Persistance.Blocks
{
    internal class EmptyBlock : IIndexNode
    {
        public string Name
        {
            get { return ""; }
            set { }
        }

        public Folder Parent
        {
            get { return null; }
            set {  }
        }
    }

}