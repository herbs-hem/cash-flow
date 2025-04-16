using System.Data;

namespace CashFlow.Infrastructure.Persistence.Sql.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}