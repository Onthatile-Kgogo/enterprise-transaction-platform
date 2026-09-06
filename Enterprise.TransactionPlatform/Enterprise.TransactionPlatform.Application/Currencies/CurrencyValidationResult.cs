namespace Enterprise.TransactionPlatform.Application.Currencies
{
    public sealed class CurrencyValidationResult
    {
        public bool IsValid { get; }

        public string? Error { get; }

        private CurrencyValidationResult(bool isValid, string? error)
        {
            IsValid = isValid;
            Error = error;
        }

        public static CurrencyValidationResult Success()
        {
            return new CurrencyValidationResult(true, null);
        }

        public static CurrencyValidationResult Failure(string error)
        {
            return new CurrencyValidationResult(false, error);
        }
    }
}
