using System.Collections.Generic;

namespace VFSBase.Implementation
{
    internal class BlockAllocation
    {
        private long _nextFreeBlock = 1;
        private readonly LinkedList<long> _freeList = new LinkedList<long>();

        public long Allocate()
        {
            if (_freeList.First == null) return _nextFreeBlock++;

            var first = _freeList.First.Value;
            _freeList.RemoveFirst();
            return first;
        }

        public void Free(long blockNumber)
        {
            _freeList.AddFirst(blockNumber);
        }
    }
}