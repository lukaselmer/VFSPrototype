namespace VFSBase.Implementation
{
    internal class BlockAllocation
    {
        private long _nextFreeBlock = 1;

        public long Allocate()
        {
            return _nextFreeBlock++;
        }
    }
}