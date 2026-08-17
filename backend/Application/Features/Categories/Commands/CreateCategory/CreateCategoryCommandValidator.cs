using FluentValidation;

namespace Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public CreateCategoryCommandValidator() 
    {
        RuleFor(category => category.CategoryName)
            .NotEmpty().WithMessage("Category name is required.")
            .MinimumLength(3).WithMessage("Category name must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Category name must not exceed 100 characters.");

        RuleFor(x => x)
            .Must(x =>
                (x.ImageContent is null && x.ImageFileName is null) ||
                (x.ImageContent is not null && !string.IsNullOrWhiteSpace(x.ImageFileName)))
            .WithMessage("Image content and image file name must be provided together.");

        When(x => x.ImageContent is not null, () =>
        {
            RuleFor(x => x.ImageContent!)
                .Must(stream => stream.Length > 0)
                    .WithMessage("Image file cannot be empty.")
                .Must(stream => stream.Length <= MaxFileSizeBytes)
                    .WithMessage("Image must be smaller than 5MB.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.ImageFileName), () =>
        {
            RuleFor(x => x.ImageFileName!)
                .Must(fileName => AllowedExtensions.Contains(
                    Path.GetExtension(fileName).ToLowerInvariant()))
                    .WithMessage("Only .jpg, .jpeg, .png, and .webp files are supported.");
        });
    }
}
