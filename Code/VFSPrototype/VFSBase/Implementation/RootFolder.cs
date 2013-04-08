using System;

namespace VFSBase.Implementation
{
    internal class RootFolder : Folder
    {
        public override long BlockNumber
        {
            get { return 0; }
            set { throw new ArgumentException("Cannot set the block number of the root folder"); }
        }
    }
}
