using System;
using System.Linq;

namespace Walsh
{
    public static class Walsh
    {
        public static Func<double, sbyte> Get(int n)
        {
            var binary = BaseConverter.ToBinary(n).ToArray();

            return x =>
            {
                var xBinary = BaseConverter.ToBinary(x, binary.Length);
                var result = binary.Zip(xBinary, (nb, xb) => nb * xb).Sum();

                return (sbyte) (result % 2 == 0 ? 1 : -1);
            };
        }

        public static Func<double, double> Get(int r, int k)
        {
            if (k < r) return x => Math.Pow(x, k) / Factorial(k);

            var w = Get(k - r);

            if (r == 1) return x => GetOneK(k)(x);

            var factorial = 1.0 / Factorial(r - 1);
            
            return x =>
            {
                return factorial * Integral.Trapezoid(t => Math.Pow(x - t, r - 1) * w(t), 0, x, 1 << 6);
            };
        }

        public static Func<double, double> GetOneK(int k)
        {
            switch (k)
            {
                case 0:
                    return x => 1;
                case 1:
                    return x => x;
            }
            k--;
            
            var t = (int) Math.Log(k, 2);
            var i = k - (1 << t);
            var pow2T = Math.Pow(2, t);
            var w = Get(i);

            return x => w(x) * 1 / pow2T * GetOneTwo(pow2T * x);
        }

        private static double GetOneTwo(double x)
        {
            x -= (int) x;
            return x < .5 ? x : 1 - x;
        }

        private static int Factorial(int n)
        {
            return n < 1 ? 1 : Enumerable.Range(1, n).Aggregate((i, j) => i * j);
        }

        public static sbyte[,] GetMatrix(int k, WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return ordering switch
            {
                WalshMatrixOrdering.Dyadic => GetDyadicMatrix(k),
                WalshMatrixOrdering.Natural => GetNaturalMatrix(k),
                _ => GetDyadicMatrix(k)
            };
        }

        private static sbyte[,] GetDyadicMatrix(int k)
        {
            var prevMatrix = new sbyte[,] {{1}};
            var order = 0;
            while (order < k)
            {
                ++order;
                var prevLength = prevMatrix.GetLength(0);
                var nextMatrix = new sbyte[2 * prevLength, 2 * prevLength];
                for (var i = 0; i < prevLength; i++)
                for (var j = 0; j < prevLength; j++)
                {
                    nextMatrix[2 * i, j] = nextMatrix[2 * i + 1, j] =
                        nextMatrix[2 * i, prevLength + j] = prevMatrix[i, j];
                    nextMatrix[2 * i + 1, prevLength + j] = (sbyte) -prevMatrix[i, j];
                }

                prevMatrix = nextMatrix;
                if (order >= k) break;
            }

            return prevMatrix;
        }

        private static sbyte[,] GetNaturalMatrix(int k)
        {
            if (k == 0)
            {
                return new sbyte[,] {{ 1 }};
            }

            var length = 1 << k;
            var result = new sbyte[length, length];
            var h = GetNaturalMatrix(k - 1);
            var hLength = h.GetLength(0);
            for (var i = 0; i < hLength; i++)
            {
                for (var j = 0; j < hLength; j++)
                {
                    result[i, j] = 
                        result[i + hLength, j] = 
                            result[i, j + hLength] = h[i, j];
                    result[i + hLength, j + hLength] = (sbyte) -h[i, j];
                }
            }
            
            return result;
        }
    }
}