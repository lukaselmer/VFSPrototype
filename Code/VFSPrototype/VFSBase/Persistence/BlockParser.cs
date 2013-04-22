using System;
using System.Linq;
using System.Text;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence.Blocks;

namespace VFSBase.Persistence
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

            if (typeByte == FolderType) return ParseFolder(bb);

            if (typeByte == FileType) return ParseFile(bb);

            return EmptyBlock.Get();
        }

        private Folder ParseFolder(byte[] bb)
        {
            if (bb.Length != _options.BlockSize) return null;

            var name = ExtractName(bb);

            return new Folder(name)
                       {
                           BlocksCount = BitConverter.ToInt64(bb, _options.NameLength + 1),
                           IndirectNodeNumber = BitConverter.ToInt64(bb, sizeof(long) + _options.NameLength + 1)
                       };
        }

        public RootFolder ParseRootFolder(byte[] bb)
        {
            var f = ParseFolder(bb);
            return new RootFolder { BlocksCount = f.BlocksCount, IndirectNodeNumber = f.IndirectNodeNumber };
        }

        private VFSFile ParseFile(byte[] bb)
        {
            if (bb.Length != _options.BlockSize) return null;

            var name = ExtractName(bb);

            return new VFSFile(name)
            {
                BlocksCount = BitConverter.ToInt64(bb, _options.NameLength + 1),
                IndirectNodeNumber = BitConverter.ToInt64(bb, sizeof(long) + _options.NameLength + 1),
                LastBlockSize = BitConverter.ToInt32(bb, sizeof(long) * 2 + _options.NameLength + 1)
            };
        }

        public IndirectNode ParseIndirectNode(byte[] bb)
        {
            var referenceAmount = _options.BlockSize / _options.BlockReferenceSize;
            var references = new long[referenceAmount];
            for (var i = 0; i < referenceAmount; i++)
            {
                var blockNumber = BitConverter.ToInt64(bb, i * _options.BlockReferenceSize);
                if (blockNumber == 0) break;
                references[i] = blockNumber;
            }

            return new IndirectNode(references);
        }

        private string ExtractName(byte[] bb)
        {
            var nameBytes = bb.Skip(1).Take(_options.NameLength).ToArray();
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

        private static byte TypeBit(IIndexNode node)
        {
            if (node is Folder) return FolderType;
            if (node is VFSFile) return FileType;
            throw new ArgumentException("is not a folder or file", "node");
        }

        public byte[] NodeToBytes(IIndexNode node)
        {
            var bb = new byte[_options.BlockSize];
            bb[0] = TypeBit(node);

            WriteNameToBuffer(ref bb, node.Name);
            var offset = _options.NameLength + 1;

            BitConverter.GetBytes(node.BlocksCount).CopyTo(bb, offset);
            offset += sizeof(long);

            BitConverter.GetBytes(node.IndirectNodeNumber).CopyTo(bb, offset);
            offset += sizeof(long);

            var file= node as VFSFile;
            if (file != null) BitConverter.GetBytes(file.LastBlockSize).CopyTo(bb, offset);

            return bb;
        }

        public byte[] NodeToBytes(IndirectNode indirectNode)
        {
            var bb = new byte[_options.BlockSize];

            for (var i = 0; i < _options.ReferencesPerIndirectNode; i++)
            {
                var blockNumber = indirectNode[i];
                if (blockNumber == 0) break;
                BitConverter.GetBytes(blockNumber).CopyTo(bb, i * _options.BlockReferenceSize);
            }

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
