using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Application.DependencyInjection;
using Enterprise.TransactionPlatform.Infrastructure.Currencies;
using Enterprise.TransactionPlatform.Infrastructure.DependencyInjection;

namespace Enterprise.TransactionPlatform.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services
                .AddOptions<CurrencyOptions>()
                .Bind(builder.Configuration.GetSection(CurrencyOptions.SectionName))
                .Validate(
                    options => options.Supported is { Length: > 0 },
                    "At least one supported currency must be configured.")
                .Validate(
                    options => options.Supported.All(code =>
                        !string.IsNullOrWhiteSpace(code) &&
                        code.Trim().Length == 3 &&
                        code.Trim().All(char.IsLetter)),
                    "All supported currencies must contain exactly 3 letters.")
                .ValidateOnStart();

            builder.Services.AddOpenApi();
            builder.Services.AddControllers();
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddSingleton<ISupportedCurrencyProvider, SupportedCurrencyProvider>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint(
                        "/openapi/v1.json",
                        "Enterprise Transaction Platform API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
