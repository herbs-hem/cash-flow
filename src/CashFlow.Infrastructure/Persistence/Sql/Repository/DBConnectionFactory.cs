using System.Data;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using MySql.Data.MySqlClient;

namespace CashFlow.Infrastructure.Persistence.Sql.Repository;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public DbConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}