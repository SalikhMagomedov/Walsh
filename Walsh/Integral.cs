using System;
using System.Linq;

namespace Walsh
{
    public static class Integral
    {
        public static double Rectangular(Func<double, double> f, double from = 0, double to = 1, int n = 1)
        {
            var sum = .0;
            var delta = (to - from) / n;

            for (var i = 0; i < n; i++)
            {
                var a = from + i * delta;
                var b = from + (i + 1) * delta;

                sum += (b - a) * f((a + b) / 2);
            }

            return sum;
        }

        public static double Trapezoid(Func<double, double> f, double from = 0, double to = 1, int n = 1)
        {
            var sum = .0;
            var delta = (to - from) / n;

            for (var i = 0; i < n; i++)
            {
                var a = from + i * delta;
                var b = from + (i + 1) * delta;

                sum += (b - a) * (f(a) + f(b)) / 2;
            }

            return sum;
        }

        public static double Simpson(Func<double, double> f, double from = 0, double to = 1, int n = 1)
        {
            var delta = (to - from) / n;
            return delta * (f(from) / 2 + Enumerable.Range(1, n - 1)
                .Select(i => f(from + i * delta))
                .Sum() + f(to) / 2);
        }
    }
}