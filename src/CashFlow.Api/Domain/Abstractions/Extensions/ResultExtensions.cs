using CashFlow.Api.Domain.Abstractions.Generic;

namespace CashFlow.Api.Domain.Abstractions.Extensions;

public static class ResultExtensions
{
    public static Result<T> ToResult<T>(this Result result)
    {
        if (result.IsSuccess)
            return Result<T>.Success(default!);

        return Result<T>.Failure(result.Error);
    }
}