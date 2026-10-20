namespace ProductApi.Domain.Exceptions;

public sealed class ProductNotFoundException(Guid productId) : KeyNotFoundException($"Product '{productId}' was not found.")
{
    public Guid ProductId { get; } = productId;
}