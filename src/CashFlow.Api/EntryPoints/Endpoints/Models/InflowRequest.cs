namespace CashFlow.Api.EntryPoints.Endpoints.Models;

public sealed record InflowRequest(decimal Amount, string Description = default!);