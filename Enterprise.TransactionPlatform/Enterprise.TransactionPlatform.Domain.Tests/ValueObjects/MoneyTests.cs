using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Domain.Tests.ValueObjects
{
    public class MoneyTests
    {
        [Fact]
        public void Create_WithValidAmount_ShouldCreateMoney()
        {
            var currency = Currency.Create("ZAR");
            var money = Money.Create(100.50m, currency);

            Assert.Equal(100.50m, money.Amount);
            Assert.Equal(currency, money.Currency);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100.50)]
        public void Create_WithNonPositiveAmount_ShouldThrowArgumentOutOfRangeException(decimal amount)
        {
            var currency = Currency.Create("ZAR");
            Assert.Throws<ArgumentOutOfRangeException>(() => Money.Create(amount, currency));
        }

        [Fact]
        public void Create_WithNullCurrency_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Money.Create(100m, null!));
        }

        [Fact]
        public void TwoMoneyValues_WithSameAmountAndCurrency_ShouldBeEqual()
        {
            var first = Money.Create(100m, Currency.Create("ZAR"));
            var second = Money.Create(100m, Currency.Create("zar"));

            Assert.Equal(first, second);
        }

        [Fact]
        public void TwoMoneyValues_WithDifferentAmounts_ShouldNotBeEqual()
        {
            var first = Money.Create(100m, Currency.Create("ZAR"));
            var second = Money.Create(200m, Currency.Create("ZAR"));

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void TwoMoneyValues_WithDifferentCurrencies_ShouldNotBeEqual()
        {
            var first = Money.Create(100m, Currency.Create("ZAR"));
            var second = Money.Create(100m, Currency.Create("USD"));

            Assert.NotEqual(first, second);
        }
    }
}
