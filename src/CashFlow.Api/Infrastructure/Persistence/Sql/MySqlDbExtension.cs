using Dapper;
using CashFlow.Api.Infrastructure.Persistence.Sql.Interfaces;
using CashFlow.Api.Infrastructure.Persistence.Sql.Repository;
using CashFlow.Api.Infrastructure.Settings;

namespace CashFlow.Api.Infrastructure.Persistence.Sql;

public static class MySqlDbExtension
{
    public static IServiceCollection AddSqlPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(provider =>
        new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));

        services.AddScoped<IAccountBalanceRepository, AccountBalanceRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Inicializa o banco de dados
        Task.Run(() => InitializeDatabase(services.BuildServiceProvider()));

        return services;
    }

    private static async Task InitializeDatabase(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();

            // Tabela AccountBalances
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS AccountBalances (
                    CompanyAccountId CHAR(36) PRIMARY KEY,
                    InitialBalance DECIMAL(18,2) NOT NULL,
                    FinalBalance DECIMAL(18,2) NOT NULL,
                    Date DATETIME NOT NULL
                )");

            // Tabela Transactions
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId CHAR(36) PRIMARY KEY,
                    CompanyAccountId CHAR(36) NOT NULL,
                    Amount DECIMAL(18,2) NOT NULL,
                    OperationType INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Description VARCHAR(255),
                    FOREIGN KEY (CompanyAccountId) REFERENCES AccountBalances(CompanyAccountId)
                )");

            // Índice para melhorar consultas por conta e data
            // Índice para melhorar consultas por conta e data
            var indexExists = await connection.QueryFirstOrDefaultAsync<int>(@"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.STATISTICS 
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'Transactions'
                  AND INDEX_NAME = 'idx_transactions_bankaccount_date';");

            if (indexExists == 0)
            {
                await connection.ExecuteAsync(@"
                CREATE INDEX idx_transactions_bankaccount_date
                ON Transactions(CompanyAccountId, Date);");
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
}