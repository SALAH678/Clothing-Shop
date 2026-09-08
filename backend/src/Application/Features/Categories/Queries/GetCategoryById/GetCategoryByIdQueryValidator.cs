using FluentValidation;

namespace Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryValidator : AbstractValidator<GetCategoryByIdQuery>
{
    public GetCategoryByIdQueryValidator()
    {
        RuleFor(request => request.CategoryId)
            .NotEmpty()
            .WithErrorCode("CategoryId_Is_Required")
            .WithMessage("CategoryId is required.");
    }
}
