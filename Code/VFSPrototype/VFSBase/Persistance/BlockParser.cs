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

        private const byte FolderType = 1;
        private const byte FileType = 2;

        public BlockParser(FileSystemOptions options)
        {
            _options = options;
        }

        public IIndexNode ParseBlock(byte[] bb)
        {
            if (bb.Length != _options.BlockSize) return EmptyBlock.Get();

            var typeByte = bb[0];

            if (typeByte == FolderType) return ParseDirectory(bb);

            if (typeByte == FileType) return ParseFile(bb);

            return EmptyBlock.Get();
        }

        private Folder ParseDirectory(byte[] bb)
        {
            var name = ExtractName(bb);
            // TODO: parse contents
            return new Folder(name);
        }

        private VFSFile ParseFile(byte[] bb)
        {
            var name = ExtractName(bb);
            // TODO: parse contents
            return new VFSFile(name, new byte[10]);
        }

        private static string ExtractName(byte[] bb)
        {
            var nameBytes = bb.Skip(1).Take(255).ToArray();
            return BytesToString(nameBytes);
        }

        public static byte[] StringToBytes(string s)
        {
            return Encoding.UTF8.GetBytes(s);
        }

        private static string BytesToString(byte[] nameBytes)
        {
            return Encoding.UTF8.GetString(nameBytes);
        }

    }
}
