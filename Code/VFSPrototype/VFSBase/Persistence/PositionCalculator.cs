namespace VFSBase.Persistence
{
    internal class PositionCalculator
    {
        private readonly PositionCalculatorSettings _settings;

        public PositionCalculator(PositionCalculatorSettings positionCalculatorSettings)
        {
            _settings = positionCalculatorSettings;
        }

        public PositionCalculatorSettings Settings
        {
            get { return _settings; }
        }

        public long StartBlock
        {
            get { return Settings.SuperBlockSize; }
        }

        public long CalculateBlockStart(long position)
        {
            return StartBlock + position * Settings.BlockSize;
        }
    }
}