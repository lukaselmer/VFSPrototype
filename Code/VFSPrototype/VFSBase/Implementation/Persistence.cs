using VFSBase.Interfaces;
using VFSBase.Persistance;
using VFSBase.Persistance.Blocks;

namespace VFSBase.Implementation
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

        public void Persist(IIndexNode node)
        {
            //NOTE: this could (and should) be better, right!?
            if (node is Folder) PersistFolder(node as Folder);
            else if (node is VFSFile) Persist(node as VFSFile);
        }

        public void PersistFolder(Folder folder)
        {
            _blockManipulator.WriteBlock(folder.BlockNumber, _blockParser.NodeToBytes(folder));
        }

        public void Persist(VFSFile file)
        {
            _blockManipulator.WriteBlock(file.BlockNumber, _blockParser.NodeToBytes(file));
        }

        public void Persist(IndirectNode indirectNode)
        {
            _blockManipulator.WriteBlock(indirectNode.BlockNumber, _blockParser.NodeToBytes(indirectNode));
        }
    }
}