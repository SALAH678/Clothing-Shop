using FluentValidation;

namespace Application.Features.Carts.Commands.RemoveCartItem;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(command => command.CartItemId)
            .NotEmpty().WithMessage("Cart item id is required.");
    }
}