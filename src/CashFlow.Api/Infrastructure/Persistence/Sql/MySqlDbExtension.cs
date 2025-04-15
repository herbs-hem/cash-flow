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
                    BankAccountId CHAR(36) PRIMARY KEY,
                    InitialBalance DECIMAL(18,2) NOT NULL,
                    FinalBalance DECIMAL(18,2) NOT NULL,
                    Date DATETIME NOT NULL
                )");

            // Tabela Transactions
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Transactions (
                    TransactionId CHAR(36) PRIMARY KEY,
                    BankAccountId CHAR(36) NOT NULL,
                    Amount DECIMAL(18,2) NOT NULL,
                    OperationType INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Description VARCHAR(255),
                    FOREIGN KEY (BankAccountId) REFERENCES AccountBalances(BankAccountId)
                )");

            // Índice para melhorar consultas por conta e data
            await connection.ExecuteAsync(@"
                CREATE INDEX IF NOT EXISTS idx_transactions_bankaccount_date
                ON Transactions(BankAccountId, Date)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
}