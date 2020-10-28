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
            const int k = 6;
            const double m = 100;
            
            var f = new Func<double, double>(Math.Sin);
            var g = PartialSum(f, k);
            var h = PartialSum(g, k);

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
            const int k = 2;
            var x = Enumerable.Range(0, 1 << k).Select(i => i / ((1 << k) - 1.0)).ToArray();
            
            double F(double t) => Math.Sin(6 * Math.PI * t) + t;
            double Df(double t) => 6 * Math.PI * Math.Cos(6 * Math.PI * t) + 1;
            
            var s = PartialSumOneK(F, Df, 1 << k);
            
            var actual = x.Select(d => s(d));
            var expected = x.Select(F);
            actual.Zip(expected, (a, e) => a - e)
                .Should()
                .OnlyContain(d => d < 1e-2);
        }

        [Test]
        public void Fwst()
        {
            const int k = 2;
            var x = Enumerable.Range(0, 1 << k).Select(i => i / ((1 << k) - 1.0)).ToArray();

            double F(double t) => Math.Sin(6 * Math.PI * t) + t;
            double Df(double t) => 6 * Math.PI * Math.Cos(6 * Math.PI * t) + 1;

            var c = FastWalshSobolevTransform(Df, k);
            var y = InverseFastWalshSobolevTransform(c, F(0));

            var actual = x.Select(d => y(d));
            var expected = x.Select(F);
            actual.Zip(expected, (a, e) => a - e)
                .Should()
                .OnlyContain(d => d < 1e-10);
        }
    }
}