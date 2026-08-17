using Dapper;
using Enterprise.TransactionPlatform.Application.Abstractions.Persistence;
using Enterprise.TransactionPlatform.Domain.Entities;
using Enterprise.TransactionPlatform.Domain.Enums;
using Enterprise.TransactionPlatform.Domain.ValueObjects;
using Enterprise.TransactionPlatform.Infrastructure.DependencyInjection;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.TransactionPlatform.Infrastructure.Tests.Persistence
{
    public sealed class TransactionRepositoryIntegrationTests
    {
        private const string ConnectionString = "Server=Onthatile_PC;Database=EnterpriseTransactionPlatform;Trusted_Connection=True;TrustServerCertificate=True;";

        [Fact]
        public async Task AddAsync_WithValidTransaction_ShouldPersistTransaction()
        {
            // Arrange
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:EnterpriseTransactionPlatform"] =
                        ConnectionString
                })
                .Build();

            var services = new ServiceCollection();
            services.AddInfrastructure(configuration);
            await using var serviceProvider = services.BuildServiceProvider();

            var repository = serviceProvider.GetRequiredService<ITransactionRepository>();
            var referenceValue = $"TEST-{Guid.NewGuid():N}";
            var reference = TransactionReference.Create(referenceValue);
            var currency = Currency.Create("ZAR");
            var money = Money.Create(1500.00m, currency);

            var transaction = Transaction.Create(reference, money, TransactionType.Payment, "Infrastructure integration test");

            try
            {
                // Act
                await repository.AddAsync(transaction);

                // Assert
                await using var connection = new SqlConnection(ConnectionString);
                await connection.OpenAsync();

                const string selectSql = """
                    SELECT
                        TransactionId,
                        Reference,
                        Amount,
                        Currency,
                        Type,
                        Status,
                        Description,
                        CreatedAtUtc,
                        UpdatedAtUtc
                    FROM dbo.Transactions
                    WHERE TransactionId = @TransactionId;
                    """;

                var persisted =
                    await connection.QuerySingleOrDefaultAsync<TransactionRow>(
                        selectSql,
                        new
                        {
                            transaction.TransactionId
                        });

                Assert.NotNull(persisted);
                Assert.Equal(transaction.TransactionId, persisted.TransactionId);
                Assert.Equal(transaction.Reference.Value, persisted.Reference);
                Assert.Equal(transaction.Money.Amount, persisted.Amount);
                Assert.Equal(transaction.Money.Currency.Code, persisted.Currency);
                Assert.Equal(transaction.Type.ToString(), persisted.Type);
                Assert.Equal(transaction.Status.ToString(), persisted.Status);
                Assert.Equal(transaction.Description, persisted.Description);
            }
            finally
            {
                await using var cleanupConnection = new SqlConnection(ConnectionString);
                await cleanupConnection.OpenAsync();

                const string deleteSql =
                    """
                        DELETE FROM dbo.Transactions
                        WHERE TransactionId = @TransactionId;
                    """;

                await cleanupConnection.ExecuteAsync(deleteSql, new
                {
                    transaction.TransactionId
                });
            }
        }

        private sealed class TransactionRow
        {
            public Guid TransactionId { get; init; }
            public string Reference { get; init; } = string.Empty;
            public decimal Amount { get; init; }
            public string Currency { get; init; } = string.Empty;
            public string Type { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public string? Description { get; init; }
            public DateTime CreatedAtUtc { get; init; }
            public DateTime? UpdatedAtUtc { get; init; }
        }
    }
}
