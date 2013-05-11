using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VFSBase.Exceptions;
using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence.Blocks;
using VFSBase.Persistence.Coding;
using VFSBlockAbstraction;

namespace VFSBase.Persistence
{
    internal class BlockList : IBlockList
    {
        private readonly IIndexNode _node;
        private readonly BlockAllocation _blockAllocation;
        private readonly FileSystemOptions _options;
        private readonly BlockParser _blockParser;
        private readonly BlockManipulator _blockManipulator;
        private readonly Persistence _persistence;

        // NOTE: long parameter smell. Are they all needed? If yes: refactoring "introduce parameter object".
        public BlockList(IIndexNode node, BlockAllocation blockAllocation, FileSystemOptions options, BlockParser blockParser,
                         BlockManipulator blockManipulator, Persistence persistence)
        {
            _node = node;
            _blockAllocation = blockAllocation;
            _options = options;
            _blockParser = blockParser;
            _blockManipulator = blockManipulator;
            _persistence = persistence;
        }

        public void AddReference(long reference)
        {
            //TODO: make this dynamic, so _options.IndirectionCountForIndirectNodes can be adjusted dynamically
            Debug.Assert(_options.IndirectionCountForIndirectNodes == 2, "This method works only with an indirection count of exactly 2");

            var indirectNodeNumber = _node.IndirectNodeNumber;
            if (indirectNodeNumber == 0)
            {
                _node.IndirectNodeNumber = CreateIndirectNode().BlockNumber;
            }

            var blocksCount = _node.BlocksCount;
            var refsCount = _options.ReferencesPerIndirectNode;

            var indexIndirection2 = (int)(blocksCount / (refsCount * refsCount));
            var indexIndirection1 = (int)((blocksCount - (indexIndirection2 * refsCount * refsCount)) / refsCount);
            var indexIndirection0 = (int)(blocksCount - (indexIndirection2 * refsCount * refsCount) - (refsCount * indexIndirection1));

            _node.BlocksCount += 1;
            _persistence.Persist(_node);

            var indirectNode3 = ReadIndirectNode(_node.IndirectNodeNumber);
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

        public IEnumerable<IIndexNode> AsEnumerable()
        {
            var l = new List<IIndexNode>((int)_node.BlocksCount);
            if (_node.IndirectNodeNumber == 0) return l;

            AddFromIndirectNode(ReadIndirectNode(_node.IndirectNodeNumber), l, _options.IndirectionCountForIndirectNodes);

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

        /*private void ReplaceInIndirectNode(IndirectNode indirectNode, long toBeReplaced, long toReplace, int recursion)
        {
            for (var i = 0; i < indirectNode.UsedBlockNumbers().Count(); i++)
            {
                var blockNumber = indirectNode[i];

                if (recursion == 0)
                {
                    // TODO: inspect this: is it needed?? 
                    if (blockNumber != toBeReplaced) continue;

                    indirectNode[i] = toReplace;
                    _persistence.PersistIndirectNode(indirectNode);
                    return;
                }

                ReplaceInIndirectNode(ReadIndirectNode(blockNumber), toBeReplaced, toReplace, recursion - 1);
            }
        }*/

        private void AddFromIndirectNode(IndirectNode indirectNode, List<IIndexNode> l, int recursion)
        {
            foreach (var blockNumber in indirectNode.UsedBlockNumbers())
            {
                if (recursion == 0) l.Add(ReadIndexNode(blockNumber));
                else AddFromIndirectNode(ReadIndirectNode(blockNumber), l, recursion - 1);
            }
        }

        public IEnumerable<byte[]> Blocks()
        {
            if (_node.IndirectNodeNumber == 0) yield break;

            foreach (var bytes in Blocks(ReadIndirectNode(_node.IndirectNodeNumber), _options.IndirectionCountForIndirectNodes))
            {
                yield return bytes;
            }
        }

        /// <summary>
        /// Copies the toCopy index node, and replaces the toReplace node with the replacement
        /// </summary>
        /// <param name="toCopy">To index node to copy.</param>
        /// <param name="toReplace">To node to be replaced. Can be set to null if only a node should be appended and no one should be replaced.</param>
        /// <param name="replacement">The node to replace the node toReplace. Can be set to null for the delete action.</param>
        /// <param name="newVersion"></param>
        /// <returns></returns>
        public Folder CopyReplacingReference(Folder toCopy, IIndexNode toReplace, IIndexNode replacement, long newVersion)
        {
            var toReplaceNr = toReplace == null ? 0 : toReplace.BlockNumber;
            var replacementNr = replacement == null ? 0 : replacement.BlockNumber;

            //var newBlocksCount = toCopy.BlocksCount - (replacementNr == 0 ? -1 : 0);

            var newFolder = new Folder(toCopy.Name)
                                   {
                                       //BlocksCount = newBlocksCount,
                                       PredecessorBlockNr = toCopy.BlockNumber, //toReplaceNr,
                                       BlockNumber = _blockAllocation.Allocate(),
                                       Version = newVersion
                                   };
            _persistence.Persist(newFolder);

            var b = new BlockList(newFolder, _blockAllocation, _options, _blockParser, _blockManipulator, _persistence);

            // Improve this algorithm section! We don't have to copy everything, we only have to copy the blocks that are different.
            foreach (var reference in AsEnumerable())
            {
                var blockNumber = reference.BlockNumber == toReplaceNr ? replacementNr : reference.BlockNumber;
                if (blockNumber != 0) b.AddReference(blockNumber);
            }
            if (toReplace == null && replacement != null) b.AddReference(replacementNr);

            if (replacement != null) replacement.Parent = newFolder;

            return newFolder;
        }

        private IEnumerable<byte[]> Blocks(IndirectNode indirectNode, int recursion)
        {
            foreach (var blockNumber in indirectNode.UsedBlockNumbers())
            {
                if (recursion == 0) yield return _blockManipulator.ReadBlock(blockNumber);
                else foreach (var bytes in Blocks(ReadIndirectNode(blockNumber), recursion - 1)) yield return bytes;
            }
        }

        private IIndexNode ReadIndexNode(long blockNumber)
        {
            var b = _blockParser.BytesToNode(_blockManipulator.ReadBlock(blockNumber));
            b.BlockNumber = blockNumber;
            return b;
        }
    }
}
