using System.Collections.Generic;

namespace WalshSystem
{
    public static class BaseConverter
    {
        public static IEnumerable<byte> ToBinary(int n)
        {
            var bin = new List<byte>(64);
            while (n > 0)
            {
                bin.Add((byte) (n & 1));
                n >>= 1;
            }

            return bin;
        }

        public static IEnumerable<byte> ToBinary(double x, int digitsCount)
        {
            var bin = new byte[digitsCount];
            x -= (int) x;
            for (var i = 0; i < digitsCount; i++)
            {
                x *= 2;
                bin[i] = (byte) x;
                x -= (int) x;
            }

            return bin;
        }
    }
}