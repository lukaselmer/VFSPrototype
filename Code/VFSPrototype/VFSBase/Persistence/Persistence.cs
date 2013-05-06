using VFSBase.Implementation;
using VFSBase.Interfaces;
using VFSBase.Persistence.Blocks;

namespace VFSBase.Persistence
{
    internal class Persistence
    {
        private readonly BlockParser _blockParser;
        private readonly BlockManipulator _blockManipulator;

        public Persistence(BlockParser blockParser, BlockManipulator blockManipulator)
        {
            _blockParser = blockParser;
            _blockManipulator = blockManipulator;
        }

        public void PersistIndirectNode(IndirectNode indirectNode)
        {
            _blockManipulator.WriteBlock(indirectNode.BlockNumber, _blockParser.NodeToBytes(indirectNode));
        }

        public void Persist(IIndexNode node)
        {
            _blockManipulator.WriteBlock(node.BlockNumber, _blockParser.NodeToBytes(node));
        }
    }
}
