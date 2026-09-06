using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Tests.ValueObjects
{
    public class CurrencyTests
    {
        [Fact]
        public void Create_WithLowercaseCode_ShouldNormalizeToUppercase()
        {
            var currency = Currency.Create("zar");

            Assert.Equal("ZAR", currency.Code);
        }

        [Fact]
        public void Create_WithWhitespace_ShouldTrimAndNormalize()
        {
            var currency = Currency.Create(" usd ");

            Assert.Equal("USD", currency.Code);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ZA")]
        [InlineData("ZARR")]
        public void Create_WithInvalidCode_ShouldThrowArgumentException(string code)
        {
            Assert.Throws<ArgumentException>(() => Currency.Create(code));
        }

        [Fact]
        public void TwoCurrencies_WithSameCode_ShouldBeEqual()
        {
            var first = Currency.Create("ZAR");
            var second = Currency.Create("zar");

            Assert.Equal(first, second);
        }

        [Theory]
        [InlineData("12A")]
        [InlineData("123")]
        [InlineData("Z@R")]
        public void Create_WithNonLetterCharacters_ShouldThrowArgumentException(string code)
        {
            Assert.Throws<ArgumentException>(() => Currency.Create(code));
        }
    }
}
