namespace CashFlow.Api.EntryPoints.Endpoints.Models;

public sealed record OutflowRequest(decimal Amount, string Description = default!);