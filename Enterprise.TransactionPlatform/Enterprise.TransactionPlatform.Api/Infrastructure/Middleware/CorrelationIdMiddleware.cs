using System.Diagnostics;

namespace Enterprise.TransactionPlatform.Api.Infrastructure.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        public const string ItemKey = "CorrelationId";

        private readonly RequestDelegate next;
        private readonly ILogger<CorrelationIdMiddleware> logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = GetOrCreateCorrelationId(context);

            context.Items[ItemKey] = correlationId;
            context.Response.Headers[HeaderName] = correlationId;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();

                logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId: {CorrelationId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    correlationId);
            }
        }

        private static string GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(HeaderName, out var headerValue) && !string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue.ToString().Trim();
            }

            return Guid.NewGuid().ToString("N");
        }
    }
}