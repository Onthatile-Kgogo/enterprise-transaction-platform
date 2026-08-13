namespace Enterprise.TransactionPlatform.Infrastructure.Currencies
{
    public sealed class CurrencyOptions
    {
        public const string SectionName = "Currencies";
        public string[] Supported { get; init; } = [];
    }
}
