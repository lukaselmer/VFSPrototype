namespace VFSBase.Implementation
{
    internal class BlockAllocation
    {
        private readonly FileSystemOptions _options;
        internal long _nextFreeBlock = 1;

        public BlockAllocation(FileSystemOptions options)
        {
            _options = options;
        }

        public long Allocate()
        {
            return _nextFreeBlock++;
        }
    }
}