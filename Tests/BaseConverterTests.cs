using FluentAssertions;
using NUnit.Framework;
using static WalshSystem.BaseConverter;

namespace Tests
{
    [TestFixture]
    public class BaseConverterTests
    {
        [Test]
        public void ToBinaryGetCorrectNumber()
        {
            ToBinary(4).Should().Equal(0, 0, 1);
        }

        [Test]
        public void ToBinaryCorrectDoubleNumber()
        {
            ToBinary(.625, 3).Should().Equal(1, 0, 1);
        }
    }
}