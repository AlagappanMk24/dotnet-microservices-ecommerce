namespace Ecommerce.SharedLibrary.Exceptions;

public sealed class RequestValidationException(IEnumerable<string> errors) : Exception("One or more validation errors occurred.")
{
    public IReadOnlyList<string> Errors { get; } = errors.ToArray();
}