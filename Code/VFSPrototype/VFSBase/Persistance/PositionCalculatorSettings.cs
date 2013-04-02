namespace VFSBase.Persistance
{
    public struct PositionCalculatorSettings
    {
        public ulong SuperBlockSize { get; private set; }
        public ulong BlockSize { get; private set; }
        public ulong BlockAmount { get; private set; }

        public PositionCalculatorSettings(ulong superBlockSize, ulong blockSize)
            : this()
        {
            SuperBlockSize = superBlockSize;
            BlockSize = blockSize;
        }
    }
}