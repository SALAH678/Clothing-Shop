using FluentValidation;

namespace Application.Features.Carts.Commands.AddCartItem;

public sealed class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
{
    public AddCartItemCommandValidator()
    {
        RuleFor(command => command.VariantId)
            .NotEmpty().WithMessage("Variant id is required.");

        RuleFor(command => command.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}