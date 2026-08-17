using FluentValidation;

namespace Application.Features.Categories.Commands.UnAssignSubCategoriesToCategory;

public class UnAssignSubCategoriesToCategoryCommandValidator : AbstractValidator<UnAssignSubCategoriesToCategoryCommand>
{
    public UnAssignSubCategoriesToCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("CategoryId is required");

        RuleFor(x => x.SubCategoryIds)
            .NotEmpty().WithMessage("At least one SubCategory is required");

        RuleForEach(x => x.SubCategoryIds)
            .NotEmpty().WithMessage("SubCategoryId cannot be empty");
    }
}
