namespace VFSBase.Persistence
{
    /// <summary>
    /// Provides some math utility methods on long values
    /// </summary>
    internal static class BinaryMathUtil
    {
        /// <summary>
        /// Gigabyte helper.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public static long GB(int i)
        {
            return Power2(30) * i;
        }

        /// <summary>
        /// Megabytes helper.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public static long MB(int i)
        {
            return Power2(20) * i;
        }

        /// <summary>
        /// Kilobytes helper.
        /// </summary>
        /// <param name="i">The i.</param>
        /// <returns></returns>
        public static long KB(int i)
        {
            return Power2(10) * i;
        }

        /// <summary>
        /// Calculates the power of 2^exponent
        /// </summary>
        /// <param name="exponent">The exponent.</param>
        /// <returns></returns>
        public static long Power2(int exponent)
        {
            return 1L << exponent;
        }

        /// <summary>
        /// Implements the power function for integers.
        /// </summary>
        /// <param name="_base">The _base.</param>
        /// <param name="exponent">The exponent.</param>
        /// <returns></returns>
        public static long Power(int _base, int exponent)
        {
            long ret = 1;
            for (var i = 0; i < exponent; i++) ret *= _base;
            return ret;
        }
    }
}
