using System;
using System.Collections.Generic;
using System.Linq;
using VFSBase.Exceptions;
using VFSBase.Interfaces;
using VFSBase.Persistence;
using VFSBase.Persistence.Blocks;

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
            var readBlock = _blockManipulator.ReadBlock(reference);
            var node = _blockParser.ParseIndirectNode(readBlock);
            node.BlockNumber = reference;
            return node;
        }

        private IndirectNode CreateIndirectNode()
        {
            var newNodeNumber = _blockAllocation.Allocate();
            var indirectNode = new IndirectNode(new long[_options.ReferencesPerIndirectNode]) { BlockNumber = newNodeNumber };
            _persistence.Persist(indirectNode);
            return indirectNode;
        }

        public void Delete(IIndexNode nodeToDelete)
        {
            var parentNode = _node as Folder;
            if(parentNode == null) throw new VFSException("Can only delete nodes from folders!");

            if (parentNode.BlocksCount == 0) return;

            parentNode.BlocksCount--;
            if (parentNode.BlocksCount == 0)
            {
                parentNode.IndirectNodeNumber = 0;
                _persistence.PersistFolder(parentNode);
                return;
            }

            _blockManipulator.WriteBlock(parentNode.BlockNumber, _blockParser.NodeToBytes(parentNode));

            var blocksCount = parentNode.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            var indirectNode3 = ReadIndirectNode(parentNode.IndirectNodeNumber);
            var indirectNode2 = ReadIndirectNode(indirectNode3.BlockNumbers[indexIndirection2]);
            var indirectNode1 = ReadIndirectNode(indirectNode2.BlockNumbers[indexIndirection1]);

            var refToMove = indirectNode1.BlockNumbers[indexIndirection0];

            // The file contents could be destroyed, but it is not necessary.
            // Disabled, so "Move" can be implemented simpler.
            // WriteBlock(node.BlockNumber, new byte[_options.BlockSize]);

            indirectNode1.BlockNumbers[indexIndirection0] = 0;
            _persistence.Persist(indirectNode1);

            if (refToMove == nodeToDelete.BlockNumber) return;

            ReplaceInIndirectNode(indirectNode3, nodeToDelete.BlockNumber, refToMove, 2);
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

        public IEnumerable<IIndexNode> AsEnumerable()
        {
            var l = new List<IIndexNode>((int)_node.BlocksCount);
            if (_node.IndirectNodeNumber == 0) return l;

            AddFromIndirectNode(ReadIndirectNode(_node.IndirectNodeNumber), l, 2);

            var folder = _node as Folder;
            if (folder != null) l.ForEach(f => f.Parent = folder);

            return l;
        }

        private void AddFromIndirectNode(IndirectNode indirectNode, List<IIndexNode> l, int recursion)
        {
            foreach (var blockNumber in indirectNode.BlockNumbers)
            {
                if (blockNumber == 0) return;

                if (recursion == 0) l.Add(ReadIndexNode(blockNumber));
                else AddFromIndirectNode(ReadIndirectNode(blockNumber), l, recursion - 1);
            }
        }

        private IIndexNode ReadIndexNode(long blockNumber)
        {
            var b = _blockParser.BytesToNode(_blockManipulator.ReadBlock(blockNumber));
            b.BlockNumber = blockNumber;
            return b;
        }

        public bool Exists(string name)
        {
            return AsEnumerable().Any(i => i.Name == name);
        }

        public IIndexNode Find(string name)
        {
            return AsEnumerable().FirstOrDefault(i => i.Name == name);
        }
    }
}