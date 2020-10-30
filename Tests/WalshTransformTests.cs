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
        [Test]
        public void ForwardTransform_InputArray_ReturnCorrectResult()
        {
            ForwardTransform(new[] {1.0, 0, 1, 0}).Should().Equal(2, 0, 2, 0);
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
            FastWalshHadamardTransform(new[] {1, 0, 1, 0, 0, 1, 1, 0.0})
                .Should().Equal(4, 2, 0, -2, 0, 2, 0, 2);
        }

        [Test]
        public void InverseFastWalshHadamardTransformTest()
        {
            var x = new[] {1, 0, 1, 0, 0, 1, 1, 0.0};
            var y = FastWalshHadamardTransform(x).ToArray();
            
            InverseFastWalshHadamardTransform(y).Should().Equal(x);
        }

        [Test]
        public void PartialSumOneKTest()
        {
            const int n = (1 << 2) + 1;
            var x = Enumerable.Range(0, n).Select(i => i / (n - 1.0)).ToArray();
            
            double F(double t) => Math.Sin(6 * Math.PI * t) + t;
            double Df(double t) => 6 * Math.PI * Math.Cos(6 * Math.PI * t) + 1;
            
            var s = PartialSumOneK(F, Df, n);
            
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

            double F(double t) => Math.Sin(6 * Math.PI * t) + t;
            double Df(double t) => 6 * Math.PI * Math.Cos(6 * Math.PI * t) + 1;

            var c = FastWalshSobolevTransform(Df, k);
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
            
            var c = FastWalshTransform(f);

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
            var x = Enumerable.Range(0, n).Select(i => i / (n - 1.0)).ToArray();

            double F(double t) => Math.Sin(6 * Math.PI * t) + t;
            double Df(double t) => 6 * Math.PI * Math.Cos(6 * Math.PI * t) + 1;
            
            var c1 = FastWalshSobolevTransform(Df, k)
                .Select(d => d / n);
            var c2 = Enumerable.Range(1, n)
                .Select(i => GetCoefficientOneK(F, Df, i, n));

            c1.Should().Equal(c2, (a, e) => Math.Abs(a - e) < 1e-10);
        }
    }
}