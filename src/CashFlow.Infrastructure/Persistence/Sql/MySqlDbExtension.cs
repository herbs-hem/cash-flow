using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using CashFlow.Infrastructure.Persistence.Sql.Repository;
using CashFlow.Infrastructure.Settings;
using Dapper;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.Infrastructure.Persistence.Sql;

public static class MySqlDbExtension
{

    public static IServiceCollection AddSqlPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IDbConnectionFactory>(provider =>
        new DbConnectionFactory(provider.GetRequiredService<IAppSettings>().DatabaseSettings.ConnectionString));

        services
            .AddScoped<IAccountBalanceRepository, AccountBalanceRepository>()
            .AddScoped<ITransactionRepository, TransactionRepository>()
            .AddScoped<IConsolidatedReportRepository, ConsolidatedReportRepository>();

        // Inicializa o banco de dados
        Task.Run(() => InitializeDatabase(services.BuildServiceProvider()));

        return services;
    }
    
    static async Task InitializeDatabase(IServiceProvider services)
    {
        try
        {
            using var scope = services.CreateScope();
            var connection = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>().CreateConnection();

            // Tabela AccountBalances
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS AccountBalances (
                    EntityIdentifier CHAR(36) PRIMARY KEY,
                    InitialBalance DECIMAL(18,2) NOT NULL,
                    FinalBalance DECIMAL(18,2) NOT NULL,
                    Date DATETIME NOT NULL
                )");

            // Tabela Transactions
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS Transactions (
                    Id CHAR(36) PRIMARY KEY,
                    EntityIdentifier CHAR(36) NOT NULL,
                    Amount DECIMAL(18,2) NOT NULL,
                    OperationType INT NOT NULL,
                    Date DATETIME NOT NULL,
                    Description VARCHAR(255),
                    FOREIGN KEY (EntityIdentifier) REFERENCES AccountBalances(EntityIdentifier)
                )");

            // �ndice para melhorar consultas por conta e data
            //await connection.ExecuteAsync(@"
            //    CREATE INDEX IF NOT EXISTS idx_transactions_entityidentifier_date ON Transactions(EntityIdentifier, Date)");

            // Verifica e cria índice idx_transactions_entityidentifier_date
            var existingIndex = await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT INDEX_NAME
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = 'Transactions'
                AND INDEX_NAME = 'idx_transactions_entityidentifier_date'");

            if (string.IsNullOrEmpty(existingIndex))
            {
                await connection.ExecuteAsync(@"
                    CREATE INDEX idx_transactions_entityidentifier_date 
                    ON Transactions(EntityIdentifier, Date)");
            }


            // Tabela Users
            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id CHAR(36) PRIMARY KEY,
                Email VARCHAR(255) NOT NULL UNIQUE,
                Username VARCHAR(100) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                CreatedAt DATETIME NOT NULL
            )");

            // Tabela Roles
            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Roles (
                Id CHAR(36) PRIMARY KEY,
                Name VARCHAR(50) NOT NULL UNIQUE
            )");

            // Tabela UserRoles (relacionamento)
            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS UserRoles (
                UserId CHAR(36) NOT NULL,
                RoleId CHAR(36) NOT NULL,
                PRIMARY KEY (UserId, RoleId),
                FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
                FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE
            )");

            // Inserir roles padrão
            await connection.ExecuteAsync(@"
            INSERT IGNORE INTO Roles (Id, Name) VALUES 
            (UUID(), 'Admin'),
            (UUID(), 'Customer'),
            (UUID(), 'Manager')");

            //await connection.ExecuteAsync(@"
            //    CREATE INDEX IF NOT EXISTS idx_users_email ON Users(Email)");

            // Verifica e cria índice idx_users_email
            existingIndex = await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT INDEX_NAME
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = 'Users'
                AND INDEX_NAME = 'idx_users_email'");

            if (string.IsNullOrEmpty(existingIndex))
            {
                await connection.ExecuteAsync(@"
                    CREATE INDEX idx_users_email ON Users(Email)");
            }

            //await connection.ExecuteAsync(@"
            //    CREATE INDEX IF NOT EXISTS idx_users_username ON Users(Username)");

            // Verifica e cria índice idx_users_username
            existingIndex = await connection.QueryFirstOrDefaultAsync<string>(@"
                SELECT INDEX_NAME
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                AND TABLE_NAME = 'Users'
                AND INDEX_NAME = 'idx_users_username'");

            if (string.IsNullOrEmpty(existingIndex))
            {
                await connection.ExecuteAsync(@"
                    CREATE INDEX idx_users_username ON Users(Username)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database initialization failed: {ex.Message}");
            throw;
        }
    }
}