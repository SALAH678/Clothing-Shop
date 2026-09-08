
namespace Application.Features.Categories.Dtos
{
    public class CategoryDto
    {
        public Guid Id { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public List<CategoryDto>? Subcategories { get; init; }
    }
}

