using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Domain.ValueObjects;

namespace Enterprise.TransactionPlatform.Application.Currencies
{
    public sealed class CurrencyValidator
    {
        private readonly ISupportedCurrencyProvider _currencyProvider;

        public CurrencyValidator(ISupportedCurrencyProvider currencyProvider)
        {
            ArgumentNullException.ThrowIfNull(currencyProvider);
            _currencyProvider = currencyProvider;
        }

        public async Task<CurrencyValidationResult> ValidateAsync(Currency currency, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(currency);

            var isSupported = await _currencyProvider.IsSupportedAsync(currency.Code, cancellationToken);
            if (!isSupported)
            {
                return CurrencyValidationResult.Failure(
                    $"Currency '{currency.Code}' is not supported.");
            }

            return CurrencyValidationResult.Success();
        }
    }
}
