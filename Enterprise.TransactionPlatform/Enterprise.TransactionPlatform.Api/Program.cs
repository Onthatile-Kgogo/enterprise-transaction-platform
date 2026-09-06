using Enterprise.TransactionPlatform.Api.Configuration;
using Enterprise.TransactionPlatform.Api.Infrastructure.Exceptions;
using Enterprise.TransactionPlatform.Api.Infrastructure.Middleware;
using Enterprise.TransactionPlatform.Application.DependencyInjection;
using Enterprise.TransactionPlatform.Infrastructure.Currencies;
using Enterprise.TransactionPlatform.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using System.Threading.RateLimiting;

namespace Enterprise.TransactionPlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddOptions<RateLimitOptions>()
                .Bind(builder.Configuration.GetSection(RateLimitOptions.SectionName))
                .Validate(options => options.PermitLimit > 0, "Rate limit permit limit must be greater than zero.")
                .Validate(options => options.WindowSeconds > 0, "Rate limit window must be greater than zero seconds.")
                .Validate(options => options.QueueLimit >= 0, "Rate limit queue limit cannot be negative.")
                .ValidateOnStart();

            builder.Services
                .AddOptions<CurrencyOptions>()
                .Bind(builder.Configuration.GetSection(CurrencyOptions.SectionName))
                .Validate(options => options.Supported is { Length: > 0 }, "At least one supported currency must be configured.")
                .Validate(options => options.Supported.All(code => !string.IsNullOrWhiteSpace(code) && code.Trim().Length == 3 && code.Trim().All(char.IsLetter)), "All supported currencies must contain exactly 3 letters.")
                .ValidateOnStart();

            builder.Services.AddOpenApi();
            builder.Services.AddControllers();

            builder.Services.AddRateLimiter(options =>
            {
                var rateLimitOptions =
                    builder.Configuration
                        .GetSection(RateLimitOptions.SectionName)
                        .Get<RateLimitOptions>()
                    ?? throw new InvalidOperationException("Rate limiting configuration was not found.");

                options.GlobalLimiter =
                    PartitionedRateLimiter.Create<HttpContext, string>(
                        httpContext => RateLimitPartition.GetFixedWindowLimiter(partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                                factory: _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = rateLimitOptions.PermitLimit,
                                    Window = TimeSpan.FromSeconds(rateLimitOptions.WindowSeconds),
                                    QueueLimit = rateLimitOptions.QueueLimit
                                }));

                options.RejectionStatusCode =
                    StatusCodes.Status429TooManyRequests;
            });

            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "Enterprise Transaction Platform API v1");
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseExceptionHandler();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapHealthChecks("/health/live",
                new HealthCheckOptions
                {
                    Predicate = _ => false
                });
            app.MapHealthChecks("/health/ready",
                new HealthCheckOptions
                {
                    Predicate = check => check.Tags.Contains("ready")
                });

            app.MapControllers();

            app.Run();
        }
    }
}