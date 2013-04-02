namespace VFSBase.Persistance
{
    internal class BinaryMathUtil
    {
        public static ulong MB(int i)
        {
            return KB(1) * KB(1) * Power2(i);
        }

        private static ulong KB(int i)
        {
            return Power2(10);
        }

        public static ulong Power2(int exponent)
        {
            const ulong b = 2;
            ulong res = 1;
            // Slow variant, could be done in O(log(n)), this is O(n)
            for (var i = 0; i < exponent; i++) res *= b;

            return res;
        }
    }
}