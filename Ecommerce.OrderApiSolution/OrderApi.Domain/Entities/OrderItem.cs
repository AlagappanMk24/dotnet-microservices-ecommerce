using Ecommerce.SharedLibrary.Domain;

namespace OrderApi.Domain.Entities;

public sealed class OrderItem : BaseEntity
{
    private OrderItem() { }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal LineTotal => decimal.Round(UnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);

    internal static OrderItem Create(Guid productId, string productName, string sku, int quantity, decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("ProductId is required.", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU is required.", nameof(sku));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        if (unitPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");

        return new OrderItem
        {
            ProductId = productId,
            ProductName = productName.Trim(),
            Sku = sku.Trim(),
            Quantity = quantity,
            UnitPrice = decimal.Round(unitPrice, 2)
        };
    }
}