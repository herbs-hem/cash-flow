using MediatR;
using CashFlow.Api.Domain.Abstractions;

namespace CashFlow.Api.Application.QueryHandlers;

public interface IQueryHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    Task<Result> ValidateAsync(TRequest request);
}