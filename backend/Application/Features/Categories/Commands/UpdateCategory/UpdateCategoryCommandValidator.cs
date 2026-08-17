
using FluentValidation;

namespace Application.Features.Categories.Commands.UpdateCategory;

public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public UpdateCategoryCommandValidator()
    {
        RuleFor(category => category.categoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(category => category.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(3).WithMessage("Category name must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.")
            .When(category => category.CategoryName is not null);

        RuleFor(x => x.NewImageContent)
           .Must(stream => stream!.Length > 0)
               .WithMessage("Image file cannot be empty.")
           .Must(stream => stream!.Length <= MaxFileSizeBytes)
               .WithMessage("Image must be smaller than 5MB.")
               .When(x => x.NewImageContent is not null);

        RuleFor(x => x.ImageFileName)
            .NotEmpty()
                .WithMessage("Image file name is required.")
            .Must(fileName => AllowedExtensions.Contains(
                Path.GetExtension(fileName!).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg, .png, and .webp files are supported.")
                .When(x => x.NewImageContent is not null);
    }
}
