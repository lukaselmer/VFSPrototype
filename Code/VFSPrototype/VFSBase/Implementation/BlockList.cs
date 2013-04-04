using System;
using VFSBase.Interfaces;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

namespace VFSBase.Implementation
{
    internal class BlockList
    {
        private readonly IIndexNode _node;
        private readonly BlockAllocation _blockAllocation;
        private readonly FileSystemOptions _options;
        private readonly BlockParser _blockParser;
        private readonly BlockManipulator _blockManipulator;
        private readonly Persistence _persistance;

        public BlockList(IIndexNode node, BlockAllocation blockAllocation, FileSystemOptions options, BlockParser blockParser, BlockManipulator blockManipulator, Persistence persistance)
        {
            _node = node;
            _blockAllocation = blockAllocation;
            _options = options;
            _blockParser = blockParser;
            _blockManipulator = blockManipulator;
            _persistance = persistance;
        }

        public void Add(long reference)
        {
            var parentFolder = _node as Folder;
            
            if(parentFolder == null) throw new NotImplementedException();

            var indirectNodeNumber = parentFolder.IndirectNodeNumber;
            if (indirectNodeNumber == 0)
            {
                parentFolder.IndirectNodeNumber = CreateIndirectNode().BlockNumber;
            }

            var blocksCount = parentFolder.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            parentFolder.BlocksCount += 1;
            _persistance.PersistFolder(parentFolder);

            var indirectNode3 = ReadIndirectNode(parentFolder.IndirectNodeNumber);
            if (indirectNode3.BlockNumbers[indexIndirection2] == 0)
            {
                indirectNode3.BlockNumbers[indexIndirection2] = CreateIndirectNode().BlockNumber;
                _persistance.Persist(indirectNode3);
            }

            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            if (indirectNode2.BlockNumbers[indexIndirection1] == 0)
            {
                indirectNode2.BlockNumbers[indexIndirection1] = CreateIndirectNode().BlockNumber;
                _persistance.Persist(indirectNode2);
            }

            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);
            indirectNode1.BlockNumbers[indexIndirection0] = reference;
            _persistance.Persist(indirectNode1);
        }

        private IndirectNode ReadIndirectNode(long reference)
        {
            return FileSystem.ReadIndirectNodeStatic(_blockManipulator, _blockParser, reference);
        }

        private IndirectNode CreateIndirectNode()
        {
            var newNodeNumber = _blockAllocation.Allocate();
            var indirectNode = new IndirectNode(new long[_options.ReferencesPerIndirectNode]) { BlockNumber = newNodeNumber };
            _persistance.Persist(indirectNode);
            return indirectNode;
        }
    }
}