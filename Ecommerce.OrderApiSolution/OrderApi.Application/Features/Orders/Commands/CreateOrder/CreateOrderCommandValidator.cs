using FluentValidation;

namespace OrderApi.Application.Features.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.Items).NotNull().NotEmpty().WithMessage("At least one item is required.").Must(x => x == null || x.Count <= 100).WithMessage("An order cannot contain more than 100 items.");
        RuleForEach(x => x.Items).ChildRules(item => { item.RuleFor(x => x.ProductId).NotEmpty(); item.RuleFor(x => x.Quantity).InclusiveBetween(1, 10000); });
        RuleFor(x => x.Currency).NotEmpty().Length(3).Matches("^[A-Za-z]{3}$");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}