using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Walsh;
using static Walsh.WalshTransform;

namespace Tests
{
    [TestFixture]
    public class WalshTransformTests
    {
        private static double F(double x) =>
            x * x;

        private static double Df(double x) =>
            2 * x;

        [Test]
        public void ForwardTransform_InputArray_ReturnCorrectResult()
        {
            ForwardTransform(new[] {1.0, 0, 1, 0}).Should().Equal(2.0, 0, 2.0, 0);
        }

        [Test]
        public void InverseTransform_InputCoefficientArray_ReturnCorrectResult()
        {
            InverseTransform(new[] {2.0, 0, 2.0, 0}).Should().Equal(1, 0, 1, 0);
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
            FastWalshTransform(new[] {1, 0, 1, 0, 0, 1, 1, 0.0}, WalshMatrixOrdering.Natural)
                .Should().Equal(4, 2, 0, -2, 0, 2, 0, 2);
        }

        [Test]
        public void InverseFastWalshHadamardTransformTest()
        {
            var x = new[] {1, 0, 1, 0, 0, 1, 1, 0.0};
            var y = FastWalshTransform(x, WalshMatrixOrdering.Natural).ToArray();
            
            InverseFastWalshTransform(y, WalshMatrixOrdering.Natural).Should().Equal(x);
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void PartialSumOneKTest(int k)
        {
            var n = 1 << k;
            var x = Enumerable.Range(0, n).Select(i => i / (n - 1.0)).ToArray();

            var c = FastWalshSobolevTransform(F, k);
            var s = PartialSumOneK(F(0), c);
            
            var actual = x.Select(d => s(d));
            var expected = x.Select(F);

            actual.Should().Equal(expected, (a, e) => Math.Abs(a - e) < 1e-3);
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void Fwst(int k)
        {
            var x = Enumerable.Range(0, 1 << k).Select(i => i / ((1 << k) - 1.0)).ToArray();

            var c = FastWalshSobolevTransform(F, k).ToArray();
            var y = InverseFastWalshSobolevTransform(c, F(0));

            var actual = x.Select(d => y(d));
            var expected = x.Select(F);
            
            actual.Should().Equal(expected, (a, e) => Math.Abs(a - e) < 1e-3);
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

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void FastWalshTest(int k)
        {
            var n = 1 << k;

            var c1 = FastWalshSobolevTransform(F, k);
            var c2 = Enumerable.Range(1, n)
                .Select(i => GetCoefficientOneK(F, i));

            c1.Should().Equal(c2, (a, b) => Math.Abs(a - b) < 1e-10);
        }

        [Test]
        public void FastWalshSobolevTransform_MustBeEqual_ForwardTransformDF()
        {
            const int k = 4;
            const int n = 1 << k;
            
            var g = Enumerable.Range(0, n)
                .Select(i => F((i + 1.0) / n) - F((double) i / n))
                .ToArray();

            var fast = FastWalshSobolevTransform(F, k);
            var slow = ForwardTransform(g);

            fast.Should().Equal(slow);
        }
    }
}