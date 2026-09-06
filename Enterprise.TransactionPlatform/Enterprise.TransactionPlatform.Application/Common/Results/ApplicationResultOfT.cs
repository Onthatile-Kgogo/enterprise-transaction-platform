namespace Enterprise.TransactionPlatform.Application.Common.Results
{
    public sealed class ApplicationResult<T> : ApplicationResult
    {
        public T? Value { get; }

        private ApplicationResult(bool isSuccess, T? value, ApplicationError? error) : base(isSuccess, error)
        {
            Value = value;
        }

        public static ApplicationResult<T> Success(T value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return new ApplicationResult<T>(true, value, null);
        }

        public static new ApplicationResult<T> Failure(string code, string message)
        {
            return new ApplicationResult<T>(false, default, new ApplicationError(code, message));
        }
    }
}
