namespace VFSBase.Persistence
{
    /// <summary>
    /// The position calculator settings
    /// </summary>
    internal struct PositionCalculatorSettings
    {
        public long SuperBlockSize { get; private set; }
        public long BlockSize { get; private set; }

        public PositionCalculatorSettings(long superBlockSize, long blockSize)
            : this()
        {
            SuperBlockSize = superBlockSize;
            BlockSize = blockSize;
        }
    }
}
