using Enterprise.TransactionPlatform.Application.Currencies;
using Enterprise.TransactionPlatform.Application.Transactions.GetById;
using Enterprise.TransactionPlatform.Application.Transactions.GetByReference;
using Enterprise.TransactionPlatform.Application.Transactions.Submit;
using Enterprise.TransactionPlatform.Application.Transactions.UpdateStatus;
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
            services.AddScoped<GetTransactionByIdHandler>();
            services.AddScoped<GetTransactionByReferenceHandler>();
            services.AddScoped<UpdateTransactionStatusHandler>();
            services.AddScoped<SearchTransactionsHandler>();

            return services;
        }
    }
}
