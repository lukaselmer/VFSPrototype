using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistance.Blocks;

namespace VFSBase.Persistance
{
    internal class BlockParser
    {
        private readonly FileSystemOptions _options;

        public BlockParser(FileSystemOptions options)
        {
            _options = options;
        }

        public IIndexNode ParseBlock(byte[] bb)
        {
            if (bb.Length != _options.BlockSize || bb[0] == 0) return new EmptyBlock();

            var type = bb[0];
            

            return null;
        }
    }
}
