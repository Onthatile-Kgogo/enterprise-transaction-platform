using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Application.Currencies;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Tests.Currencies
{
    public class CurrencyValidatorTests
    {
        [Fact]
        public async Task ValidateAsync_WhenCurrencyIsSupported_ShouldReturnValidResult()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR", "USD");
            var validator = new CurrencyValidator(provider);
            var currency = Currency.Create("ZAR");

            // Act
            var result = await validator.ValidateAsync(currency);

            // Assert
            Assert.True(result.IsValid);
            Assert.Null(result.Error);
        }

        [Fact]
        public async Task ValidateAsync_WhenCurrencyIsNotSupported_ShouldReturnInvalidResult()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR", "USD");
            var validator = new CurrencyValidator(provider);
            var currency = Currency.Create("GBP");

            // Act
            var result = await validator.ValidateAsync(currency);

            // Assert
            Assert.False(result.IsValid);
            Assert.Equal("Currency 'GBP' is not supported.", result.Error);
        }

        [Fact]
        public async Task ValidateAsync_WhenCurrencyIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var provider = new TestSupportedCurrencyProvider("ZAR");
            var validator = new CurrencyValidator(provider);

            // Act
            var action = async () =>
                await validator.ValidateAsync(null!);

            // Assert
            await Assert.ThrowsAsync<ArgumentNullException>(action);
        }

        [Fact]
        public void Constructor_WhenProviderIsNull_ShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new CurrencyValidator(null!));
        }

        private sealed class TestSupportedCurrencyProvider : ISupportedCurrencyProvider
        {
            private readonly HashSet<string> _supportedCurrencies;

            public TestSupportedCurrencyProvider(params string[] supportedCurrencies)
            {
                _supportedCurrencies = supportedCurrencies
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            public Task<bool> IsSupportedAsync(string currencyCode, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(
                    _supportedCurrencies.Contains(currencyCode));
            }
        }
    }
}
