﻿using System.Collections.Generic;
using System.Linq;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence.Blocks;

namespace VFSBase.Persistence
{
    internal class BlockList : IBlockList
    {
        private readonly IIndexNode _node;
        private readonly BlockAllocation _blockAllocation;
        private readonly FileSystemOptions _options;
        private readonly BlockParser _blockParser;
        private readonly BlockManipulator _blockManipulator;
        private readonly Implementation.Persistence _persistence;

        // NOTE: long parameter smell. Are they all needed? If yes: refactoring "introduce parameter object".
        public BlockList(IIndexNode node, BlockAllocation blockAllocation, FileSystemOptions options, BlockParser blockParser,
                         BlockManipulator blockManipulator, Implementation.Persistence persistence)
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

            if (parentFolder == null) throw new VFSException("Not implemented yet: can only add references to folders!");

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
            _persistence.Persist(parentFolder);

            var indirectNode3 = ReadIndirectNode(parentFolder.IndirectNodeNumber);
            if (indirectNode3.IsFree(indexIndirection2))
            {
                indirectNode3[indexIndirection2] = CreateIndirectNode().BlockNumber;
                _persistence.PersistIndirectNode(indirectNode3);
            }

            var indirectNode2 = ReadIndirectNode(indirectNode3[indexIndirection2]);
            if (indirectNode2.IsFree(indexIndirection1))
            {
                indirectNode2[indexIndirection1] = CreateIndirectNode().BlockNumber;
                _persistence.PersistIndirectNode(indirectNode2);
            }

            var indirectNode1 = ReadIndirectNode(indirectNode2[indexIndirection1]);
            indirectNode1[indexIndirection0] = reference;
            _persistence.PersistIndirectNode(indirectNode1);
        }

        public void Remove(IIndexNode nodeToDelete, bool freeSpace)
        {
            var parentNode = _node as Folder;
            if (parentNode == null) throw new VFSException("Can only delete nodes from folders!");

            if (parentNode.BlocksCount == 0) return;

            parentNode.BlocksCount--;
            if (parentNode.BlocksCount == 0)
            {
                parentNode.IndirectNodeNumber = 0;
                _persistence.Persist(parentNode);
                return;
            }

            _blockManipulator.WriteBlock(parentNode.BlockNumber, _blockParser.NodeToBytes(parentNode));

            var blocksCount = parentNode.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            var indirectNode3 = ReadIndirectNode(parentNode.IndirectNodeNumber);
            var indirectNode2 = ReadIndirectNode(indirectNode3[indexIndirection2]);
            var indirectNode1 = ReadIndirectNode(indirectNode2[indexIndirection1]);

            var refToMove = indirectNode1[indexIndirection0];

            // Free space is false, when called from "Move" method, true otherwise
            if (freeSpace) FreeSpace(nodeToDelete);

            indirectNode1[indexIndirection0] = 0;
            _persistence.PersistIndirectNode(indirectNode1);

            if (refToMove == nodeToDelete.BlockNumber) return;

            ReplaceInIndirectNode(indirectNode3, nodeToDelete.BlockNumber, refToMove);
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

        public bool Exists(string name)
        {
            return AsEnumerable().Any(i => i.Name == name);
        }

        public IIndexNode Find(string name)
        {
            return AsEnumerable().FirstOrDefault(i => i.Name == name);
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
            _persistence.PersistIndirectNode(indirectNode);
            return indirectNode;
        }

        private void ReplaceInIndirectNode(IndirectNode indirectNode, long toBeReplaced, long toReplace, int recursion = 2)
        {
            for (var i = 0; i < indirectNode.UsedBlockNumbers().Count(); i++)
            {
                var blockNumber = indirectNode[i];

                if (recursion == 0)
                {
                    if (blockNumber == toBeReplaced)
                    {
                        indirectNode[i] = toReplace;
                        _persistence.PersistIndirectNode(indirectNode);
                        return;
                    }
                }
                else ReplaceInIndirectNode(ReadIndirectNode(blockNumber), toBeReplaced, toReplace, recursion - 1);
            }
        }

        private void AddFromIndirectNode(IndirectNode indirectNode, List<IIndexNode> l, int recursion)
        {
            foreach (var blockNumber in indirectNode.UsedBlockNumbers())
            {
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

        /// <summary>
        /// Frees the space and destroys the file contents.
        /// Note: File contents are not destroyed recursively.
        /// </summary>
        /// <param name="node">The node.</param>
        private void FreeSpace(IIndexNode node)
        {
            // The file contents could be destroyed, but it is not necessary.
            // This would have to be done recursivly tough.
            // This can be used to nullify a single block: WriteBlock(node.BlockNumber, new byte[_options.BlockSize]);

            if (node.IndirectNodeNumber != 0) FreeSpace(ReadIndirectNode(node.IndirectNodeNumber));

            _blockAllocation.Free(node.BlockNumber);
        }

        private void FreeSpace(IndirectNode node, int recursion = 2)
        {
            foreach (var blockNumber in node.UsedBlockNumbers())
            {
                if (recursion != 0) FreeSpace(ReadIndirectNode(blockNumber), recursion - 1);
                _blockManipulator.WriteBlock(blockNumber, new byte[_options.BlockSize]);
                _blockAllocation.Free(blockNumber);
            }
        }
    }
}