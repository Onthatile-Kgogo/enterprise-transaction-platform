using Enterprise.TransactionPlatform.Application.Currencies;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.TransactionPlatform.Application.DependencyInjection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddScoped<CurrencyValidator>();
            services.AddScoped<SubmitTransactionHandler>();

            return services;
        }
    }
}
