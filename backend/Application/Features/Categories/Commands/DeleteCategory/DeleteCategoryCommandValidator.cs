
using FluentValidation;

namespace Application.Features.Categories.Commands.DeleteCategory;

public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
{
    public DeleteCategoryCommandValidator() 
    {
        RuleFor(x => x.categoryId)
        .NotEmpty().WithMessage("Customer Id is required.");
    }
}
