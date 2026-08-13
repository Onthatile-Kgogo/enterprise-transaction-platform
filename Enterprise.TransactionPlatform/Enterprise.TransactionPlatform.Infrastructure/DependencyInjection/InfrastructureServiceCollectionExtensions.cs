using Enterprise.TransactionPlatform.Application.Abstractions.Currencies;
using Enterprise.TransactionPlatform.Infrastructure.Currencies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.TransactionPlatform.Infrastructure.DependencyInjection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);

            services
                .AddOptions<CurrencyOptions>()
                .Bind(configuration.GetSection(CurrencyOptions.SectionName))
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

            services.AddSingleton<ISupportedCurrencyProvider, SupportedCurrencyProvider>();

            return services;
        }
    }
}
