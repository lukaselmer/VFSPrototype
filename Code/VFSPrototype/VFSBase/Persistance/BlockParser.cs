﻿using System;
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

            if (typeByte == FolderType) return ParseFolder(bb);

            if (typeByte == FileType) return ParseFile(bb);

            return EmptyBlock.Get();
        }

        public Folder ParseFolder(byte[] bb)
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

        public VFSFile ParseFile(byte[] bb)
        {
            if (bb.Length != _options.BlockSize) return null;

            var name = ExtractName(bb);
            // TODO: parse contents
            return new VFSFile(name, new byte[10]);
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

        public byte[] NodeToBytes(Folder folder)
        {
            var bb = new byte[_options.BlockSize];
            bb[0] = FolderType;
            WriteNameToBuffer(ref bb, folder.Name);
            BitConverter.GetBytes(folder.BlocksCount).CopyTo(bb, _options.NameLength + 1);
            BitConverter.GetBytes(folder.IndirectNodeNumber).CopyTo(bb, sizeof(long) + _options.NameLength + 1);
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

        public byte[] NodeToBytes(IndirectNode indirectNode)
        {
            var bb = new byte[_options.BlockSize];

            for (var i = 0; i < _options.ReferencesPerIndirectNode; i++)
            {
                var blockNumber = indirectNode.BlockNumbers[i];
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
