using System.Data;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}