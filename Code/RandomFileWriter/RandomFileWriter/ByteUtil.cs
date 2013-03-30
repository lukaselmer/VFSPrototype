namespace RandomFileWriter
{
    public class ByteUtil
    {
        public static int IntPow(int _base, int exponent)
        {
            if (exponent == 0) return 1;

            var res = 1;
            // This is slow - it could be done in O(log(n))
            for (var i = 0; i < exponent; ++i) res *= _base;
            return res;
        }
    }
}