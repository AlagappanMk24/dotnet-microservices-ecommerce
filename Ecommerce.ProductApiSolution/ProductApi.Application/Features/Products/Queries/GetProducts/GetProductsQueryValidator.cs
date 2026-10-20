using FluentValidation;

namespace ProductApi.Application.Features.Products.Queries.GetProducts;

public sealed class GetProductsQueryValidator
    : AbstractValidator<GetProductsQuery>
{
    private static readonly string[] AllowedSortFields =
    [
        "name",
        "sku",
        "price",
        "createdAt"
    ];

    public GetProductsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);

        RuleFor(x => x.Search).MaximumLength(200);

        RuleFor(x => x.Category).MaximumLength(100);

        RuleFor(x => x.SortBy)
            .Must(value => AllowedSortFields.Contains(
                value, StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                $"SortBy must be one of: {string.Join(", ", AllowedSortFields)}.");
    }
}