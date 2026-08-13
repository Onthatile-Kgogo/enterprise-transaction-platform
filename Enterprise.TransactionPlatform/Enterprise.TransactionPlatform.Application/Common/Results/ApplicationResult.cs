namespace Enterprise.TransactionPlatform.Application.Common.Results
{
    public class ApplicationResult
    {
        public bool IsSuccess { get; }

        public ApplicationError? Error { get; }

        protected ApplicationResult(bool isSuccess, ApplicationError? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }

        public static ApplicationResult Success()
        {
            return new ApplicationResult(true, null);
        }

        public static ApplicationResult Failure(string code, string message)
        {
            return new ApplicationResult(false,
                new ApplicationError(code, message));
        }
    }
}
