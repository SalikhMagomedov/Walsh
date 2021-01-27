using System;
using System.Collections.Generic;
using System.Linq;

namespace Walsh
{
    public static class WalshTransform
    {
        public static Func<double, double> PartialSum(Func<double, double> func, int n)
        {
            return x => Enumerable.Range(0, n)
                .Select(i => func(i / (n - 1.0)) * Walsh.Get(i)(Math.Min(x, 1 - 1.0 / n)))
                .Sum();
        }

        public static Func<double, double> PartialSumOneK(double f0, IEnumerable<double> coefficients)
        {
            IEnumerable<(double c, Func<double, double> w)> data =
                coefficients.Select((c, i) => (c, Walsh.GetOneK(i + 1)));

            return x => f0 + data.Select(d => d.c * d.w(x)).Sum();
        }

        public static double GetCoefficientOneK(Func<double, double> f, int k)
        {
            if (k <= 1) return f(1) - f(0);

            k--;

            var n = (int) Math.Log(k, 2) + 1;
            var w = Walsh.GetMatrix(n);
            var pow2N = Math.Pow(2, n);

            var result = 0.0;
            for (var i = 0; i < pow2N; i++)
            {
                result += w[k, i] * (f((i + 1) / pow2N) - f(i / pow2N));
            }

            return result;
        }

        public static IEnumerable<double> ForwardTransform(IReadOnlyList<double> y,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            var length = y.Count;

            var k = (int) Math.Log(length, 2);
            var w = Walsh.GetMatrix(k, ordering);
            for (var i = 0; i < length; i++)
            {
                var s = 0.0;
                for (var j = 0; j < length; j++)
                {
                    s += y[j] * w[i, j];
                }

                yield return s;
            }
        }

        public static IEnumerable<double> InverseTransform(IReadOnlyList<double> coefficients,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return ForwardTransform(coefficients, ordering).Select(d => d / coefficients.Count);
        }

        public static IEnumerable<double> FastWalshTransform(IReadOnlyList<double> array,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            static double[] BinaryReversedArray(IReadOnlyList<double> array)
            {
                static int BinaryInverse(int number, int n)
                {
                    var result = 0;

                    for (int i = 1, pow2K = 1 << (n - 1); i <= n; i++, pow2K >>= 1)
                    {
                        if (number == 0) break;
                
                        result += number / pow2K * (1 << (i - 1));
                        number %= pow2K;
                    }
            
                    return result;
                }

                var result = new double[array.Count];
                var n = (int) Math.Log(result.Length, 2);

                for (var i = 0; i < result.Length; i++)
                {
                    result[BinaryInverse(i, n)] = array[i];
                }

                return result;
            }
            
            static IEnumerable<double> FastWalshHadamardTransform(IReadOnlyCollection<double> vector)
            {
                var result = vector.ToArray();

                var h = 1;
                while (h < vector.Count)
                {
                    for (var i = 0; i < vector.Count; i += h * 2)
                    {
                        for (var j = i; j < i + h; j++)
                        {
                            var x = result[j];
                            var y = result[j + h];

                            result[j] = x + y;
                            result[j + h] = x - y;
                        }
                    }

                    h *= 2;
                }

                return result;
            }

            return FastWalshHadamardTransform(ordering switch
            {
                WalshMatrixOrdering.Natural => array,
                _ => BinaryReversedArray(array)
            });
        }

        public static IEnumerable<double> InverseFastWalshTransform(IReadOnlyList<double> coefficients,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return FastWalshTransform(coefficients, ordering).Select(d => d / coefficients.Count);
        }

        public static IEnumerable<double> FastWalshSobolevTransform(Func<double, double> f, int k,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            var n = 1 << k;

            var arr = Enumerable.Range(0, n)
                .Select(i => f((i + 1.0) / n) - f((double) i / n))
                .ToArray();

            return FastWalshTransform(arr, ordering);
        }

        public static Func<double, double> InverseFastWalshSobolevTransform(IReadOnlyList<double> coefficients,
            double f0, WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            var g = InverseFastWalshTransform(coefficients, ordering).ToArray();
            var s = new double[g.Length + 1];

            s[0] = f0;

            for (var i = 0; i < g.Length; i++)
            {
                s[i + 1] = s[i] + g[i];
            }

            return x =>
            {
                switch (x)
                {
                    case <= 0:
                        return s[0];
                    case >= 1:
                        return s[g.Length];
                }

                var index = (int) (x * g.Length);

                if (Math.Abs(index - x * g.Length) < double.Epsilon)
                {
                    return s[index];
                }
                
                var lp = (double) index / g.Length;
                var ls = s[index];
                var rs = s[index + 1];
                var p = (x - lp) * g.Length;
                
                return ls + (rs - ls) * p;
            };
        }
    }
}