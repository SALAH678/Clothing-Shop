
namespace Application.Features.Categories.Dtos
{
    public record CategoryDto(
        Guid Id,
        string CategoryName,
        string? ImageUrl,
        List<CategoryDto>? Subcategories = null);
}

