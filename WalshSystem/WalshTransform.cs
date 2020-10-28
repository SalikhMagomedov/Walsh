using System;
using System.Linq;

namespace WalshSystem
{
    public static class WalshTransform
    {
        public static Func<double, double> PartialSum(Func<double, double> func, int k)
        {
            var n = 1 << k;
            
            return x => Enumerable.Range(0, n)
                .Select(i => func(i / (n - 1.0)) * Walsh.Get(i)(Math.Min(x, 1 - 1.0 / n)))
                .Sum();

            // return x => Enumerable.Range(0, n)
            //     .Select(i => i / (double) n)
            //     .Select(t => func(t) * Walsh.Get((int) (x * n))(t))
            //     .Sum();
        }
        
        public static double[] ForwardTransform(double[] y, WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            var length = y.Length;
            var result = new double[length];

            var k = (int) Math.Log(length, 2);
            var w = Walsh.GetMatrix(k, ordering);
            for (var i = 0; i < length; i++)
            {
                var s = 0.0;
                for (var j = 0; j < length; j++)
                {
                    s += y[j] * w[i, j];
                }

                result[i] = s;
            }

            return result;
        }

        public static double[] InverseTransform(double[] coefficients,
            WalshMatrixOrdering ordering = WalshMatrixOrdering.Dyadic)
        {
            return ForwardTransform(coefficients, ordering).Select(x => x / coefficients.Length).ToArray();
        }

        public static double[] FastWalshHadamardTransform(double[] array)
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

        public static double[] InverseFastWalshHadamardTransform(double[] coefficients)
        {
            return FastWalshHadamardTransform(coefficients).Select(x => x / coefficients.Length).ToArray();
        }

        private static double GetCoefficientOneK(Func<double, double> f, Func<double, double> df, int k)
        {
            return k < 1 
                ? f(0) 
                : Integral.Rectangular(t => df(t) * Walsh.Get(k - 1)(t), n: 1 << 1);

            // return Integral.Trapezoid(t => k > 0 ? fs(t) *  Walsh.Get(0, k - 1)(t) : 0, 0, 1, 1 << k);
        }

        public static Func<double, double> PartialSumOneK(Func<double, double> f, Func<double, double> fs, int n)
        {
            // return x => Enumerable.Range(0, n)
            //     .Select(k => GetCoefficientOneK(k, fs) * Walsh.Get(1, k)(x))
            //     .Sum();

            var c = Enumerable.Range(0, n).Select(i => GetCoefficientOneK(f, fs, i)).ToArray();

            return x => Enumerable.Range(0, n)
                .Select(k => c[k] * Walsh.GetOneK(k)(x))
                .Sum();
        }

        public static double[] FastWalshSobolevTransform(Func<double, double> df, int k)
        {
            var n = 1 << k;

            var arr = Enumerable.Range(0, n)
                .Select(i => df(i / (n - 1.0)))
                .ToArray();

            return FastWalshHadamardTransform(arr);
        }

        public static Func<double, double> InverseFastWalshSobolevTransform(double[] coefficients, double f0)
        {
            var df = InverseFastWalshHadamardTransform(coefficients);
            var s = new double[df.Length + 1];

            s[0] = f0;

            for (var i = 0; i < df.Length; i++)
            {
                s[i + 1] = s[i] + df[i] / df.Length;
            }

            return x =>
            {
                if (x <= 0) return s[0];
                if (x >= 1) return s[s.Length - 1];

                var index = (int) (x * df.Length);
                var lp = (double) index / df.Length;
                var rp = (index + 1.0) / df.Length;
                var ls = s[index];
                var rs = s[index + 1];
                var p = (x - lp) / (rp - lp);
                
                return ls + (rs - ls) * p;
            };
        }
    }
}