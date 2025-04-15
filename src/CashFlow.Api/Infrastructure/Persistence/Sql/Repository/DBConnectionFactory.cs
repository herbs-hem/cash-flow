using MySql.Data.MySqlClient;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;
using System.Data;

namespace CashFlow.Api.Infrastructure.Persistence.Sql.Repository;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}