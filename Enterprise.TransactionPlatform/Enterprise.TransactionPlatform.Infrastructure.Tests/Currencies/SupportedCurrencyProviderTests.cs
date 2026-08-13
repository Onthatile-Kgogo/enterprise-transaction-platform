using Enterprise.TransactionPlatform.Infrastructure.Currencies;
using Microsoft.Extensions.Options;

namespace Enterprise.TransactionPlatform.Infrastructure.Tests.Currencies
{
    public class SupportedCurrencyProviderTests
    {
        [Fact]
        public async Task IsSupportedAsync_WhenCurrencyIsConfigured_ShouldReturnTrue()
        {
            // Arrange
            var provider = CreateProvider("ZAR", "USD", "GBP");

            // Act
            var result = await provider.IsSupportedAsync("ZAR");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsSupportedAsync_WhenCurrencyIsNotConfigured_ShouldReturnFalse()
        {
            // Arrange
            var provider = CreateProvider("ZAR", "USD");

            // Act
            var result = await provider.IsSupportedAsync("GBP");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsSupportedAsync_WhenCurrencyUsesDifferentCase_ShouldReturnTrue()
        {
            // Arrange
            var provider = CreateProvider("ZAR");

            // Act
            var result = await provider.IsSupportedAsync("zar");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsSupportedAsync_WhenCurrencyContainsWhitespace_ShouldReturnTrue()
        {
            // Arrange
            var provider = CreateProvider("ZAR");

            // Act
            var result = await provider.IsSupportedAsync("  ZAR  ");

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task IsSupportedAsync_WhenCurrencyIsEmptyOrWhitespace_ShouldReturnFalse(
            string currencyCode)
        {
            // Arrange
            var provider = CreateProvider("ZAR");

            // Act
            var result = await provider.IsSupportedAsync(currencyCode);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsSupportedAsync_WhenConfigurationContainsWhitespace_ShouldNormalizeConfiguredValues()
        {
            // Arrange
            var provider = CreateProvider(" zar ");

            // Act
            var result = await provider.IsSupportedAsync("ZAR");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsSupportedAsync_WhenConfigurationContainsEmptyValues_ShouldIgnoreThem()
        {
            // Arrange
            var provider = CreateProvider("", " ", "ZAR");

            // Act
            var result = await provider.IsSupportedAsync("ZAR");

            // Assert
            Assert.True(result);
        }

        private static SupportedCurrencyProvider CreateProvider(params string[] supportedCurrencies)
        {
            var options = Options.Create(
                new CurrencyOptions
                {
                    Supported = supportedCurrencies
                });

            return new SupportedCurrencyProvider(options);
        }
    }
}
