using System.Collections.Generic;

namespace WalshSystem
{
    public static class BaseConverter
    {
        public static IEnumerable<byte> ToBinary(int n)
        {
            while (n > 0)
            {
                yield return (byte) (n & 1);
                
                n >>= 1;
            }
        }

        public static IEnumerable<byte> ToBinary(double x, int digitsCount)
        {
            x -= (int) x;
            for (var i = 0; i < digitsCount; i++)
            {
                x *= 2;
                
                yield return (byte) x;
                
                x -= (int) x;
            }
        }
    }
}