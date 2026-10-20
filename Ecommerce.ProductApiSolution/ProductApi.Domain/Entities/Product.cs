using Ecommerce.SharedLibrary.Domain;

namespace ProductApi.Domain.Entities
{
    public sealed class Product : BaseEntity
    {
        private Product() { }
        public string Sku { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;
        public string? Description { get; private set; }
        public string Category { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public int StockQuantity { get; private set; }
        public string Currency { get; private set; } = "USD";
        public bool IsActive { get; private set; } = true;

        public static Product Create(
            string sku,
            string name,
            string? description,
            string category,
            decimal price,
            int stockQuantity,
            string currency)
        {
            Validate(sku, name, category, price, stockQuantity, currency);

            return new Product
            {
                Sku = sku.Trim(),
                Name = name.Trim(),
                Description = NormalizeDescription(description),
                Category = category.Trim(),
                Price = price,
                StockQuantity = stockQuantity,
                Currency = currency.Trim().ToUpperInvariant(),
                IsActive = true
            };
        }

        public void UpdateDetails(
            string name,
            string? description,
            string category,
            decimal price,
            int stockQuantity,
            string currency,
            bool isActive)
        {
            if (IsDeleted)
                throw new InvalidOperationException(
                    "A deleted product cannot be updated.");

            Validate(Sku, name, category, price, stockQuantity, currency);

            Name = name.Trim();
            Description = NormalizeDescription(description);
            Category = category.Trim();
            Price = price;
            StockQuantity = stockQuantity;
            Currency = currency.Trim().ToUpperInvariant();
            IsActive = isActive;

            MarkUpdated();
        }

        public void SoftDelete()
        {
            if (IsDeleted)
                return;

            IsActive = false;
            MarkDeleted();
        }

        private static void Validate(
            string sku,
            string name,
            string category,
            decimal price,
            int stockQuantity,
            string currency)
        {
            if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 64)
                throw new ArgumentException(
                    "SKU is required and must not exceed 64 characters.");

            if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 200)
                throw new ArgumentException(
                    "Product name is required and must not exceed 200 characters.");

            if (string.IsNullOrWhiteSpace(category) ||
                category.Trim().Length > 100)
                throw new ArgumentException(
                    "Category is required and must not exceed 100 characters.");

            if (price < 0 || price > 999999999999.99m)
                throw new ArgumentOutOfRangeException(nameof(price));

            if (stockQuantity < 0)
                throw new ArgumentOutOfRangeException(nameof(stockQuantity));

            if (string.IsNullOrWhiteSpace(currency) ||
                currency.Trim().Length != 3 ||
                !currency.Trim().All(char.IsLetter))
            {
                throw new ArgumentException(
                    "Currency must be a three-letter code.");
            }
        }

        private static string? NormalizeDescription(string? description)
        {
            return string.IsNullOrWhiteSpace(description)
                ? null
                : description.Trim();
        }
    }
}