namespace Enterprise.TransactionPlatform.Api.Configuration
{
    public sealed class RateLimitOptions
    {
        public const string SectionName = "RateLimiting";
        public int PermitLimit { get; init; }
        public int WindowSeconds { get; init; }
        public int QueueLimit { get; init; }
    }
}
