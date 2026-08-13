using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Microsoft.Extensions.Options;

namespace Enterprise.TransactionPlatform.Infrastructure.Currencies
{
    public sealed class SupportedCurrencyProvider : ISupportedCurrencyProvider
    {
        private readonly HashSet<string> _supportedCurrencies;

        public SupportedCurrencyProvider(IOptions<CurrencyOptions> options)
        {
            ArgumentNullException.ThrowIfNull(options);

            _supportedCurrencies = options.Value.Supported
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim().ToUpperInvariant())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> IsSupportedAsync(string currencyCode, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return Task.FromResult(false);
            }

            var normalizedCode = currencyCode
                .Trim()
                .ToUpperInvariant();

            return Task.FromResult(
                _supportedCurrencies.Contains(normalizedCode));
        }
    }
}
