using Enterprise.TransactionPlatform.Infrastructure.Persistence.Abstractions;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Enterprise.TransactionPlatform.Infrastructure.Persistence.Connections
{
    internal sealed class SqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public SqlConnectionFactory(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionString));
            }

            _connectionString = connectionString;
        }

        public DbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
