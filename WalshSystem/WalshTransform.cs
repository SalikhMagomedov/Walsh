using System;
using System.Collections.Generic;
using System.Linq;

namespace WalshSystem
{
    public static class WalshTransform
    {
        public static Func<double, double> PartialSum(Func<double, double> func, int n)
        {
            return x => Enumerable.Range(0, n)
                .Select(i => func(i / (n - 1.0)) * Walsh.Get(i)(Math.Min(x, 1 - 1.0 / n)))
                .Sum();
        }

        private static IEnumerable<double> BaseTransform(IReadOnlyList<double> y, WalshMatrixOrdering ordering)
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

        public static IEnumerable<double> ForwardTransform(IReadOnlyList<double> y, WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return BaseTransform(y, ordering).Select(d => d / y.Count).ToArray();
        }

        public static IEnumerable<double> InverseTransform(double[] coefficients,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return BaseTransform(coefficients, ordering);
        }

        private static IEnumerable<double> BaseFastWalshHadamardTransform(double[] array)
        {
            var result = new double[array.Length];
            array.CopyTo(result, 0);

            var h = 1;
            while (h < result.Length)
            {
                for (var i = 0; i < result.Length; i += h * 2)
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

        public static IEnumerable<double> FastWalshHadamardTransform(double[] array)
        {
            return BaseFastWalshHadamardTransform(array).Select(d => d / array.Length);
        }

        public static IEnumerable<double> InverseFastWalshHadamardTransform(double[] coefficients)
        {
            return BaseFastWalshHadamardTransform(coefficients);
        }

        private static int BinaryInverse(int number, int n)
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

        private static double[] BinaryReversedArray(IReadOnlyList<double> array)
        {
            var result = new double[array.Count];
            var n = (int) Math.Log(result.Length, 2);

            for (var i = 0; i < result.Length; i++)
            {
                result[BinaryInverse(i, n)] = array[i];
            }

            return result;
        }

        public static IEnumerable<double> FastWalshTransform(double[] array)
        {
            var result = BinaryReversedArray(array);

            return FastWalshHadamardTransform(result);
        }

        public static IEnumerable<double> InverseFastWalshTransform(double[] coefficients)
        {
            var result = BinaryReversedArray(coefficients);
            
            return BaseFastWalshHadamardTransform(result);
        }

        public static double GetCoefficientOneK(Func<double, double> f, Func<double, double> df, int k, int n = 1 << 6)
        {
            return Integral.Rectangular(t => df(t) * Walsh.Get(k - 1)(t), n: n);
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

        public static Func<double, double> PartialSumOneK(Func<double, double> f, int n)
        {
            IEnumerable<(double c, Func<double, double> w)> data = Enumerable.Range(1, n)
                .Select(i => (GetCoefficientOneK(f, i), Walsh.GetOneK(i)));
            
            return x => f(0) + data.Select(d => d.c * d.w(x)).Sum();
        }

        public static Func<double, double> PartialSumOneK(double f0, IEnumerable<double> coefficients)
        {
            IEnumerable<(double c, Func<double, double> w)> data =
                coefficients.Select((c, i) => (c, Walsh.GetOneK(i + 1)));

            return x => f0 + data.Select(d => d.c * d.w(x)).Sum();
        }

        public static IEnumerable<double> FastWalshSobolevTransform(Func<double, double> f, int k)
        {
            var n = 1 << k;

            var arr = Enumerable.Range(0, n)
                .Select(i => (f((i + 1.0) / n) - f((double) i / n)) * n)
                .ToArray();

            return FastWalshTransform(arr);
        }

        public static Func<double, double> InverseFastWalshSobolevTransform(double[] coefficients, double f0)
        {
            var df = InverseFastWalshTransform(coefficients).ToArray();
            var s = new double[df.Length + 1];

            s[0] = f0;

            for (var i = 0; i < df.Length; i++)
            {
                s[i + 1] = s[i] + df[i] / df.Length;
            }

            return x =>
            {
                if (x <= 0) return s[0];
                if (x >= 1) return s[df.Length];

                var index = (int) (x * df.Length);

                if (Math.Abs(index - x * df.Length) < double.Epsilon)
                {
                    return s[index];
                }
                
                var lp = (double) index / df.Length;
                var ls = s[index];
                var rs = s[index + 1];
                var p = (x - lp) * df.Length;
                
                return ls + (rs - ls) * p;
            };
        }
    }
}