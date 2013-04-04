using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VFSBase.Persistence.Blocks
{
    internal class IndirectNode
    {
        private long[] BlockNumbers { get; set; }

        public long BlockNumber { get; set; }

        public IndirectNode(long[] blockNumbers)
        {
            BlockNumbers = blockNumbers;
        }

        public bool IsFree(long index)
        {
            return BlockNumbers[index] == 0;
        }

        public IEnumerable<long> UsedBlockNumbers()
        {
            return BlockNumbers.Where(n => n != 0);
        }

        public long this[int index]
        {
            get { return BlockNumbers[index]; }
            set { BlockNumbers[index] = value; }
        }
    }
}