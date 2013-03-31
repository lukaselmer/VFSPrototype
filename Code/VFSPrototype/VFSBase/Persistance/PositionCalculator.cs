namespace VFSBase.Persistance
{
    public class PositionCalculator
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

        public ulong StartBlock
        {
            get { return Settings.SuperBlockSize; }
        }

        public ulong CalculateBlockStart(ulong position)
        {
            return StartBlock + position * Settings.BlockSize;
        }
    }
}