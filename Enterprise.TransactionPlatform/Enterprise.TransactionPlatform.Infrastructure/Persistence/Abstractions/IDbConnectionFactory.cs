using System.Data.Common;

namespace Enterprise.TransactionPlatform.Infrastructure.Persistence.Abstractions
{
    internal interface IDbConnectionFactory
    {
        DbConnection CreateConnection();
    }
}
