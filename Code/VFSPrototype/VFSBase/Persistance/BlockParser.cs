using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public IIndexNode BytesToNode(byte[] bb)
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
            return new string(Encoding.UTF8.GetString(nameBytes).TakeWhile(c => c != '\0').ToArray());
        }

        public byte[] NodeToBytes(Folder folder)
        {
            var bb = new byte[_options.BlockSize];
            bb[0] = FolderType;
            WriteNameToBuffer(ref bb, folder.Name);
            //TODO: do some more things...
            return bb;
        }

        public byte[] NodeToBytes(VFSFile file)
        {
            var bb = new byte[_options.BlockSize];
            bb[0] = FileType;
            WriteNameToBuffer(ref bb, file.Name);
            //TODO: do some more things...
            return bb;
        }

        private void WriteNameToBuffer(ref byte[] bb, string name)
        {
            var nameBytes = StringToBytes(name);
            if (nameBytes.Length > _options.NameLength) throw new VFSException("Name is too long");
            nameBytes.CopyTo(bb, 1);
        }
    }
}
