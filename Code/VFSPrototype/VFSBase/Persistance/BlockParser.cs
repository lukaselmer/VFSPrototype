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
        private static IDictionary<int, Type> types = new Dictionary<int, Type> { { 1, typeof(Folder) }, { 2, typeof(VFSFile) } };

        public BlockParser(FileSystemOptions options)
        {
            _options = options;
        }

        public IIndexNode ParseBlock(byte[] bb)
        {
            if (bb.Length != _options.BlockSize || !types.ContainsKey(bb[0])) return EmptyBlock.Get();

            var type = types[bb[0]];


            return null;
        }
    }
}
