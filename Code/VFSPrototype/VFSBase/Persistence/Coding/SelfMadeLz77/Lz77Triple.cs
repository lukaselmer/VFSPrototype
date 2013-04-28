namespace VFSBase.Persistence.Coding.SelfMadeLz77
{
    internal class Lz77Triple
    {
        private readonly int _matchLocation;
        private readonly byte _following;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lz77Triple"/> class.
        /// </summary>
        /// <param name="matchLocation">The match location.</param>
        /// <param name="matchLength">Length of the match.</param>
        /// <param name="following">The following.</param>
        internal Lz77Triple(int matchLocation, int matchLength, int following)
        {
            _matchLocation = (matchLocation << 4) | (matchLength >> 4);
            MatchLength = matchLength & Lz77Constants.LookAheadWindow;
            _following = (byte)following;
        }

        /// <summary>
        /// Gets the match location.
        /// </summary>
        /// <value>
        /// The match location.
        /// </value>
        public int MatchLocation
        {
            get { return _matchLocation; }
        }

        /// <summary>
        /// Gets or sets the length of the match.
        /// </summary>
        /// <value>
        /// The length of the match.
        /// </value>
        public int MatchLength { get; set; }

        /// <summary>
        /// Gets the following.
        /// </summary>
        /// <value>
        /// The following.
        /// </value>
        public byte Following
        {
            get { return _following; }
        }
    }
}