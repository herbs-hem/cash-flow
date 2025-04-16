using CashFlow.Domain.Entities;
using CashFlow.Infrastructure.Persistence.Sql.Interfaces;
using Dapper;

namespace CashFlow.Infrastructure.Persistence.Sql.Repository;

public class ConsolidatedReportRepository : IConsolidatedReportRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ConsolidatedReportRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IList<ConsolidateDetails>> GetConsolidatedDetailsAsync(
        Guid companyAccountId, 
        DateTime initialDate, 
        DateTime endDate)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = @"
            WITH BalanceChanges AS (
                SELECT 
                    t.Id,
                    t.EntityIdentifier,
                    t.Date,
                    t.Description,
                    t.OperationType,
                    t.Amount,
                    ab.InitialBalance,
                    SUM(CASE 
                            WHEN t.OperationType = 0 THEN -t.Amount 
                            ELSE t.Amount 
                        END) OVER (
                        PARTITION BY t.EntityIdentifier 
                        ORDER BY t.Date, t.Id
                        ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
                    ) AS RunningBalance,
                    ROW_NUMBER() OVER (
                        PARTITION BY t.EntityIdentifier 
                        ORDER BY t.Date DESC, t.Id DESC
                    ) AS RowNum
                FROM Transactions t
                JOIN AccountBalances ab ON t.EntityIdentifier = ab.EntityIdentifier
                WHERE t.EntityIdentifier = @CompanyAccountId
                AND t.Date BETWEEN @InitialDate AND @EndDate
            )
            SELECT 
                EntityIdentifier,
                Date,
                Description,
                OperationType,
                Amount,
                InitialBalance,
                (InitialBalance + RunningBalance) AS Balance
            FROM BalanceChanges
            ORDER BY Date DESC, RowNum DESC";

        var results = await connection.QueryAsync<ConsolidateDetails>(query, new
        {
            CompanyAccountId = companyAccountId,
            InitialDate = initialDate,
            EndDate = endDate
        });

        return results.AsList();
    }

    public async Task<ConsolidateDetails?> GetLastConsolidatedDetailAsync(Guid companyAccountId)
    {
        using var connection = _connectionFactory.CreateConnection();

        var query = @"
            SELECT 
                t.EntityIdentifier,
                t.Date,
                t.Description,
                t.OperationType,
                t.Amount,
                ab.InitialBalance,
                ab.FinalBalance AS Balance
            FROM Transactions t
            JOIN AccountBalances ab ON t.EntityIdentifier = ab.EntityIdentifier
            WHERE t.EntityIdentifier = @CompanyAccountId
            ORDER BY t.Date DESC, t.Id DESC
            LIMIT 1";

        return await connection.QueryFirstOrDefaultAsync<ConsolidateDetails>(query, new
        {
            CompanyAccountId = companyAccountId
        });
    }
}