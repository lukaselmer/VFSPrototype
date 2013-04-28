namespace VFSBase.Persistence.Coding.SelfMadeLz77
{
    internal class Lz77Triple
    {
        private readonly int _matchLoc;
        private int _matchLength;
        private readonly byte _followed;


        /// <summary>
        /// Prevents a default instance of the <see cref="Lz77Triple" /> class from being created.
        /// </summary>
        /// <param name="matchLoc">The matchLoc points to the start of a match relative to current loc. Has value 0 if no match.</param>
        /// <param name="matchLength">The MatchLength specifies the length of the match 0 if there is no match.</param>
        /// <param name="charFollowed">The char followed first character that does not match.</param>
        internal Lz77Triple(int matchLoc, int matchLength, int charFollowed)
        {
            _matchLoc = (matchLoc << 4) | (matchLength >> 4);
            _matchLength = matchLength & Lz77Constants.LookForwardWindow;
            _followed = (byte)charFollowed;
        }

        public int MatchLoc
        {
            get { return _matchLoc; }
        }

        public int MatchLength
        {
            get { return _matchLength; }
            set { _matchLength = value; }
        }

        public byte followed
        {
            get { return _followed; }
        }
    }
}