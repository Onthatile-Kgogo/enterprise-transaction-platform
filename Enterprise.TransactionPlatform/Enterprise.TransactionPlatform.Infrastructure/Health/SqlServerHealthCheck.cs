using Enterprise.TransactionPlatform.Infrastructure.Persistence.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Enterprise.TransactionPlatform.Infrastructure.Health
{
    internal sealed class SqlServerHealthCheck : IHealthCheck
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SqlServerHealthCheck(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection =
                    _connectionFactory.CreateConnection();

                await connection.OpenAsync(cancellationToken);
                await using var command = connection.CreateCommand();

                command.CommandText = "SELECT 1";

                var result = await command.ExecuteScalarAsync(cancellationToken);

                return Convert.ToInt32(result) == 1
                    ? HealthCheckResult.Healthy("SQL Server is available.")
                    : HealthCheckResult.Unhealthy("SQL Server health check returned an unexpected result.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("SQL Server is unavailable.", ex);
            }
        }
    }
}
