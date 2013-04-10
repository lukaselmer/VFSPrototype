namespace VFSBase.Persistence
{
    internal static class BinaryMathUtil
    {
        public static long GB(int i)
        {
            return Power2(30) * i;
        }

        public static long MB(int i)
        {
            return Power2(20) * i;
        }

        public static long KB(int i)
        {
            return Power2(10) * i;
        }

        public static long Power2(int exponent)
        {
            return 1L << exponent;
        }
    }
}
