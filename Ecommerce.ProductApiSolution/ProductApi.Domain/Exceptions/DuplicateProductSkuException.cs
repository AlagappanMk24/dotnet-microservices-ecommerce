namespace ProductApi.Domain.Exceptions;

public sealed class DuplicateProductSkuException(string sku) : Exception($"A product with SKU '{sku}' already exists.")
{
    public string Sku { get; } = sku;
}