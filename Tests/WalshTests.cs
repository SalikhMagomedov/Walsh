using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Walsh;

using static Walsh.Walsh;

namespace Tests
{
    [TestFixture]
    public class WalshTests
    {
        private IEnumerable<double> _xArray;

        [SetUp]
        public void SetUp()
        {
            _xArray = Enumerable.Range(0, 100)
                .Select(i => i / 99.0);
        }
        
        [Test]
        public void Get_NIsZero_AlwaysOne()
        {
            var w0 = Get(0);

            _xArray.Select(x => w0(x))
                .Should()
                .OnlyContain(t => t == 1);
        }

        [Test]
        public void Get_KIsZero_AlwaysOne()
        {
            var w = Get(1, 0);

            var actual = _xArray.Select(x => w(x));

            actual.Should().OnlyContain(x => Math.Abs(x - 1) < double.Epsilon);
        }

        [Test]
        public void Get_RIsOneKIsOne_AlwaysX()
        {
            var w = Get(1, 1);

            var actual = _xArray.Select(x => w(x));

            actual.Should().Equal(_xArray, (a, e) => Math.Abs(a - e) < 1e-10);
        }

        [TestCase(0, 0)]
        [TestCase(.25, .25)]
        [TestCase(.5, .5)]
        [TestCase(.75, .25)]
        [TestCase(1, 0)]
        public void GetOneK_KIsTwo_ReturnCorrectResult(double x, double result)
        {
            GetOneK(2)(x).Should().Be(result);
        }

        [TestCase(0, 0)]
        [TestCase(.125, .125)]
        [TestCase(.25, .25)]
        [TestCase(.375, .125)]
        [TestCase(.5, 0)]
        [TestCase(.625, .125)]
        [TestCase(.75, .25)]
        [TestCase(.875, .125)]
        [TestCase(1, 0)]
        public void GetOneK_KIsThree_ReturnCorrectResult(double x, double result)
        {
            GetOneK(3)(x).Should().Be(result);
        }
        
        [TestCase(0, 0)]
        [TestCase(.125, .125)]
        [TestCase(.25, .25)]
        [TestCase(.375, .125)]
        [TestCase(.5, 0)]
        [TestCase(.625, -.125)]
        [TestCase(.75, -.25)]
        [TestCase(.875, -.125)]
        [TestCase(1, 0)]
        public void GetOneK_KIsFour_ReturnCorrectResult(double x, double result)
        {
            GetOneK(4)(x).Should().Be(result);
        }

        [Test]
        public void Get_NIsOne_ReturnCorrectResults()
        {
            var w1 = Get(1);
            var xs = Enumerable.Range(0, 100).Select(i => i / 100.0).ToArray();

            xs.Where(x => x < 0.5).Should().OnlyContain(x => w1(x) == 1);
            xs.Where(x => x > 0.5).Should().OnlyContain(x => w1(x) == -1);
        }

        [Test]
        public void Get_NIsThree_ReturnCorrectResults()
        {
            var w3 = Get(3);
            var xs = Enumerable.Range(0, 100).Select(i => i / 100.0).ToArray();

            xs.Where(x => x < .25).Should().OnlyContain(x => w3(x) == 1);
            xs.Where(x => x > .75).Should().OnlyContain(x => w3(x) == 1);
            xs.Where(x => x > .25 && x < .75).Should().OnlyContain(x => w3(x) == -1);
        }

        [TestCase(0, new[] {1})]
        [TestCase(1, new[]
        {
            1, 1,
            1, -1
        })]
        [TestCase(2, new[]
        {
            1, 1, 1, 1,
            1, 1, -1, -1,
            1, -1, 1, -1,
            1, -1, -1, 1
        })]
        public void GetMatrix_DyadicOrdering_ReturnCorrectResult(int k, int[] resultParameters)
        {
            GetMatrix(k).Should().Equal(resultParameters);
        }

        [TestCase(0, new[] {1})]
        [TestCase(1, new[]
        {
            1, 1,
            1, -1
        })]
        [TestCase(2, new[]
        {
            1, 1, 1, 1,
            1, -1, 1, -1,
            1, 1, -1, -1,
            1, -1, -1, 1
        })]
        public void GetMatrix_NaturalOrdering_ReturnCorrectResult(int k, int[] resultParameters)
        {
            GetMatrix(k, WalshMatrixOrdering.Natural).Should().Equal(resultParameters);
        }
    }
}