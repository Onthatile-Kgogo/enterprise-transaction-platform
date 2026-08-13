namespace Enterprise.TransactionPlatform.Application.Abstractions.Currencies
{
    public interface ISupportedCurrencyProvider
    {
        Task<bool> IsSupportedAsync(string currencyCode, CancellationToken cancellationToken = default);
    }
}
