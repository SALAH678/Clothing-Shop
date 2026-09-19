using FluentValidation;

namespace Application.Features.Variants.Commands.DeleteVariant;

public sealed class DeleteVariantCommandValidator : AbstractValidator<DeleteVariantCommand>
{
    public DeleteVariantCommandValidator()
    {
        RuleFor(x => x.VariantId)
            .NotEmpty().WithMessage("Variant ID is required.");
    }
}