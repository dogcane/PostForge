using PostForge.Application.Common.Exceptions;
using Resulz;

namespace PostForge.Application.Common.Extensions;

public static class OperationResultExtensions
{
    public static TEntity EnsureSuccess<TEntity>(this OperationResult<TEntity> result)
    {
        if (result.Success)
            return result.Value!;

        throw new DomainValidationException(result.Errors);
    }

    public static void EnsureSuccess(this OperationResult result)
    {
        if (!result.Success)
            throw new DomainValidationException(result.Errors);
    }
}