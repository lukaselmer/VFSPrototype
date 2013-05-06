using VFSBase.Persistence;

namespace VFSBase.Helpers
{
    /// <summary>
    /// The position calculator
    /// </summary>
    internal class PositionCalculator
    {
        /// <summary>
        /// The settings
        /// </summary>
        private readonly PositionCalculatorSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PositionCalculator"/> class.
        /// </summary>
        /// <param name="positionCalculatorSettings">The position calculator settings.</param>
        public PositionCalculator(PositionCalculatorSettings positionCalculatorSettings)
        {
            _settings = positionCalculatorSettings;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public PositionCalculatorSettings Settings
        {
            get { return _settings; }
        }

        /// <summary>
        /// Gets the start block.
        /// </summary>
        /// <value>
        /// The start block.
        /// </value>
        public long StartBlock
        {
            get { return Settings.SuperBlockSize; }
        }

        /// <summary>
        /// Calculates the block start.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public long CalculateBlockStart(long position)
        {
            return StartBlock + position * Settings.BlockSize;
        }
    }
}
