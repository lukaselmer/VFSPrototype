using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Implementation;
using VFSBase.Interfaces;

namespace VFSBase.Persistance
{
    internal class BlockParser
    {
        public static IIndexNode ParseBlock(byte[] bb, FileSystemOptions options)
        {
            if (bb.Length != options.BlockSize) return new EmptyBlock();
            return null;
        }
    }

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
