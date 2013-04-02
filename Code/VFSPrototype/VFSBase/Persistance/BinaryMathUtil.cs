namespace VFSBase.Persistance
{
    internal class BinaryMathUtil
    {
        public static long MB(int i)
        {
            return KB(1) * KB(1) * Power2(i);
        }

        public static long KB(int i)
        {
            return Power2(10);
        }

        public static long Power2(int exponent)
        {
            return 1L << exponent;
        }
    }
}