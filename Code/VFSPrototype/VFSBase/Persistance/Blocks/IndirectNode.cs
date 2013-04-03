using System.Collections.Generic;
using System.Linq;

namespace VFSBase.Persistance.Blocks
{
    internal class IndirectNode
    {
        public long[] BlockNumbers { get; set; }

        public long BlockNumber { get; set; }

        public IndirectNode(long[] blockNumbers)
        {
            BlockNumbers = blockNumbers;
        }

        public bool HasFreeNode()
        {
            return BlockNumbers.Any(n => n == 0);
        }

        public long LastUsedNodeNumber()
        {
            return BlockNumbers[BlockNumbers.Count(i => i > 0)];
        }
    }
}