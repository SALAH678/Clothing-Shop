using Domain.Common;
using Domain.Common.Results;
using Domain.Products;

namespace Domain.Categories;

public class Category : AuditableEntity
{
    public string CategoryName { get; private set; } = null!;
    public string? ImageUrl { get; private set; } 
    public Guid? ParentCategoryId { get; private set; }

    private readonly List<Category>? _subcategories = [];
    private readonly List<Product>? _products = [];

    public IReadOnlyCollection<Category>? Subcategories => _subcategories?.AsReadOnly();
    public IReadOnlyCollection<Product>? Products => _products?.AsReadOnly();

    public Category? ParentCategory { get; private set; }

    protected Category()
    {
    }

    protected Category(string categoryName, string? imageUrl, Guid? parentCategoryId)
        : base(Guid.Empty)
    {
        CategoryName = categoryName;
        ImageUrl = imageUrl;
        ParentCategoryId = parentCategoryId;
    }

    public static Result<Category> Create(string? categoryName, string? imageUrl = null, Guid? parentCategoryId = null)
    {
        Error? error = Validate(categoryName, imageUrl, parentCategoryId);

        if (error is not null)
            return error.Value;

        return new Category(categoryName!.Trim(), imageUrl!.Trim(), parentCategoryId);
    }

    public Result<Updated> Update(string? categoryName, string? imageUrl)
    {
        Error? error = Validate(categoryName, imageUrl, ParentCategoryId);

        if (error is not null)
            return error.Value;

        CategoryName = categoryName!.Trim();
        ImageUrl = imageUrl!.Trim();

        return Result.Updated;
    }

    public Result<Updated> SetParent(Guid? parentCategoryId)
    {
        if (parentCategoryId == Guid.Empty)
            return CategoryErrors.InvalidParentCategoryId;

        if (parentCategoryId == Id)
            return CategoryErrors.CannotBeOwnParent;

        ParentCategoryId = parentCategoryId;

        return Result.Updated;
    }

    public Result<Category> AddChild(string? categoryName, string? imageUrl)
    {
        Result<Category> childResult = Create(categoryName, imageUrl, Id);

        if (childResult.IsError)
            return childResult.Errors;

        _subcategories?.Add(childResult.Value);

        return childResult.Value;
    }

    public Result<Deleted> RemoveChild(Guid childCategoryId)
    {
        Category? child = _subcategories?.FirstOrDefault(category => category.Id == childCategoryId);

        if (child is null)
            return CategoryErrors.ChildCategoryNotFound;

        _subcategories?.Remove(child);

        return Result.Deleted;
    }

    private static Error? Validate(string? categoryName, string? imageUrl, Guid? parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return CategoryErrors.CategoryNameRequired;

        if(imageUrl is not null)
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return CategoryErrors.ImageUrlRequired;

            if (imageUrl.Length > 500)
                return CategoryErrors.ImageUrlTooLong;

            if (!IsValidUrl(imageUrl))
                return CategoryErrors.InvalidImageUrl;
        }

        if (parentCategoryId == Guid.Empty)
            return CategoryErrors.InvalidParentCategoryId;

        return null;
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
