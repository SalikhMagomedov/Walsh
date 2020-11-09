using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using static WalshSystem.WalshTransform;

namespace Tests
{
    [TestFixture]
    public class WalshTransformTests
    {
        private static double F(double x) =>
            // Math.Sin(6 * Math.PI * t) + t;
            x * x;

        private static double Df(double x) =>
            // 6 * Math.PI * Math.Cos(6 * Math.PI * x) + 1;
            2 * x;

        [Test]
        public void ForwardTransform_InputArray_ReturnCorrectResult()
        {
            ForwardTransform(new[] {1.0, 0, 1, 0}).Should().Equal(.5, 0, .5, 0);
        }

        [Test]
        public void InverseTransform_InputCoefficientArray_ReturnCorrectResult()
        {
            InverseTransform(new[] {.5, 0, .5, 0}).Should().Equal(1, 0, 1, 0);
        }

        [Test]
        public void PartialSum_InputFunctionFX_ReturnSameValueOnX()
        {
            const int n = 1 << 6;
            const double m = 100;
            
            var f = new Func<double, double>(Math.Sin);
            var g = PartialSum(f, n);
            var h = PartialSum(g, n);

            Enumerable.Range(0, (int) m)
                .Select(x => f(x / m) - h(x / m))
                .Should()
                .OnlyContain(x => x < 1e-2);
        }

        [Test]
        public void FastWalshHadamardTransformTest()
        {
            FastWalshHadamardTransform(new[] {1, 0, 1, 0, 0, 1, 1, 0.0})
                .Should().Equal(.5, .25, 0, -.25, 0, .25, 0, .25);
        }

        [Test]
        public void InverseFastWalshHadamardTransformTest()
        {
            var x = new[] {1, 0, 1, 0, 0, 1, 1, 0.0};
            var y = FastWalshHadamardTransform(x).ToArray();
            
            InverseFastWalshHadamardTransform(y).Should().Equal(x);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        public void PartialSumOneKTest(int k)
        {
            var n = 1 << k;
            var x = Enumerable.Range(0, n).Select(i => i / (n - 1.0)).ToArray();
            
            var s = PartialSumOneK(F, n);
            
            var actual = x.Select(d => s(d));
            var expected = x.Select(F);

            actual.Should().Equal(expected, (a, e) => Math.Abs(a - e) < 1e-2);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void Fwst(int k)
        {
            var x = Enumerable.Range(0, 1 << k).Select(i => i / ((1 << k) - 1.0)).ToArray();

            var c = FastWalshSobolevTransform(Df, k).ToArray();
            var y = InverseFastWalshSobolevTransform(c, F(0));

            var actual = x.Select(d => y(d));
            var expected = x.Select(F);
            
            actual.Should().Equal(expected, (a, e) => Math.Abs(a - e) < 1e-2);
        }

        [Test]
        public void FastWalshTransformTest()
        {
            const int k = 3;
            var x = Enumerable.Range(0, 1 << k).Select(i => i / ((1 << k) - 1.0)).ToArray();
            var f = x.Select(d => 2 * d * d).ToArray();
            
            var c = FastWalshTransform(f).ToArray();

            InverseFastWalshTransform(c).Should().Equal(f, (a, e) => Math.Abs(a - e) <= 1e-10);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        public void FastWalshTest(int k)
        {
            var n = 1 << k;

            var c1 = FastWalshSobolevTransform(F, k);
            var c2 = Enumerable.Range(1, n)
                .Select(i => GetCoefficientOneK(F, i));
            var c3 = Enumerable.Range(1, n)
                .Select(i => GetCoefficientOneK(F, Df, i));


            bool Comparison(double a, double b)
            {
                return Math.Abs(a - b) < 1e-10;
            }

            c1.Should()
                .Equal(c2, Comparison)
                .And
                .Equal(c3, Comparison);
        }
    }
}