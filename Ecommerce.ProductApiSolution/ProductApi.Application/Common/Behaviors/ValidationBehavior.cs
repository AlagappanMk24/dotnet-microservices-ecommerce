using Ecommerce.SharedLibrary.Exceptions;
using FluentValidation;
using MediatR;

namespace ProductApi.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var failures = new List<string>();

        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(
                request, cancellationToken);

            failures.AddRange(
                result.Errors
                    .Where(error => error is not null)
                    .Select(error => error.ErrorMessage));
        }

        if (failures.Count > 0)
            throw new RequestValidationException(failures);

        return await next();
    }
}