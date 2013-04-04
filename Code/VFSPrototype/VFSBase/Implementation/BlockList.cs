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
        private readonly Persistence _persistence;

        public BlockList(IIndexNode node, BlockAllocation blockAllocation, FileSystemOptions options, BlockParser blockParser, BlockManipulator blockManipulator, Persistence persistence)
        {
            _node = node;
            _blockAllocation = blockAllocation;
            _options = options;
            _blockParser = blockParser;
            _blockManipulator = blockManipulator;
            _persistence = persistence;
        }

        public void Add(long reference)
        {
            var parentFolder = _node as Folder;

            if (parentFolder == null) throw new NotImplementedException();

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
            _persistence.PersistFolder(parentFolder);

            var indirectNode3 = ReadIndirectNode(parentFolder.IndirectNodeNumber);
            if (indirectNode3.BlockNumbers[indexIndirection2] == 0)
            {
                indirectNode3.BlockNumbers[indexIndirection2] = CreateIndirectNode().BlockNumber;
                _persistence.Persist(indirectNode3);
            }

            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            if (indirectNode2.BlockNumbers[indexIndirection1] == 0)
            {
                indirectNode2.BlockNumbers[indexIndirection1] = CreateIndirectNode().BlockNumber;
                _persistence.Persist(indirectNode2);
            }

            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);
            indirectNode1.BlockNumbers[indexIndirection0] = reference;
            _persistence.Persist(indirectNode1);
        }

        private IndirectNode ReadIndirectNode(long reference)
        {
            return FileSystem.ReadIndirectNodeStatic(_blockManipulator, _blockParser, reference);
        }

        private IndirectNode CreateIndirectNode()
        {
            var newNodeNumber = _blockAllocation.Allocate();
            var indirectNode = new IndirectNode(new long[_options.ReferencesPerIndirectNode]) { BlockNumber = newNodeNumber };
            _persistence.Persist(indirectNode);
            return indirectNode;
        }

        public void Delete(long indirectNodeNumber)
        {
            var node = _node;

            if (node.Parent.BlocksCount == 0) return;

            node.Parent.BlocksCount--;
            if (node.Parent.BlocksCount == 0)
            {
                node.Parent.IndirectNodeNumber = 0;
                _persistence.PersistFolder(node.Parent);
                return;
            }

            _blockManipulator.WriteBlock(node.Parent.BlockNumber, _blockParser.NodeToBytes(node.Parent));

            var blocksCount = node.Parent.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            var indirectNode3 = ReadIndirectNode(node.Parent.IndirectNodeNumber);
            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);

            var refToMove = indirectNode1.BlockNumbers[indexIndirection0];

            // The file contents could be destroyed, but it is not necessary.
            // Disabled, so "Move" can be implemented simpler.
            // WriteBlock(node.BlockNumber, new byte[_options.BlockSize]);

            indirectNode1.BlockNumbers[indexIndirection0] = 0;
            _persistence.Persist(indirectNode1);

            if (refToMove == node.BlockNumber) return;

            ReplaceInIndirectNode(indirectNode3, node.BlockNumber, refToMove, 2);
        }


        private void ReplaceInIndirectNode(IndirectNode indirectNode, long toBeReplaced, long toReplace, int recursion)
        {
            for (var i = 0; i < indirectNode.BlockNumbers.Length; i++)
            {
                var blockNumber = indirectNode.BlockNumbers[i];
                if (blockNumber == 0) return;

                if (recursion == 0)
                {
                    if (blockNumber == toBeReplaced)
                    {
                        indirectNode.BlockNumbers[i] = toReplace;
                        _persistence.Persist(indirectNode);
                        return;
                    }
                }
                else ReplaceInIndirectNode(ReadIndirectNode(blockNumber), toBeReplaced, toReplace, recursion - 1);
            }
        }
    }
}