using Domain.Categories;
using Domain.Common;
using Domain.Common.Results;
using Domain.Products.Images;
using Domain.Products.Variants;

namespace Domain.Products;

public class Product : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal? Discount { get; private set; }
    public Guid CategoryId { get; private set; }

    private readonly List<Variant> _variants = [];
    private readonly List<Image>? _images = [];

    public IReadOnlyCollection<Variant> Variants => _variants.AsReadOnly();
    public IReadOnlyCollection<Image>? Images => _images?.AsReadOnly();

    public Category Category { get; private set; } = null!;

    protected Product()
    {
    }

    protected Product(string name, string? description, decimal basePrice, decimal? discount, Guid categoryId)
        : base(Guid.Empty)
    {
        Name = name;
        Description = description;
        BasePrice = basePrice;
        Discount = discount;
        CategoryId = categoryId;
    }

    public static Result<Product> Create(string? name, string? description, decimal basePrice, decimal? discount, Guid categoryId)
    {
        Error? error = Validate(name, description, basePrice, discount, categoryId);

        if (error is not null)
            return error.Value;

        return new Product(name!.Trim(), description!.Trim(), basePrice, discount, categoryId);
    }

    public Result<Updated> Update(string? name, string? description, decimal basePrice, decimal discount, Guid categoryId)
    {
        Error? error = Validate(name, description, basePrice, discount, categoryId);

        if (error is not null)
            return error.Value;

        Name = name!.Trim();
        Description = description?.Trim();
        BasePrice = basePrice;
        Discount = discount;
        CategoryId = categoryId;

        return Result.Updated;
    }

    public Result<Variant> AddVariant(string? size, string? color, int stockQuantity)
    {
        bool alreadyExists = _variants.Any(variant =>
            string.Equals(variant.Size, size?.Trim(), StringComparison.OrdinalIgnoreCase) &&
            string.Equals(variant.Color, color?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (alreadyExists)
            return ProductErrors.VariantAlreadyExists;

        Result<Variant> variantResult = Variant.Create(Id, size, color, stockQuantity);

        if (variantResult.IsError)
            return variantResult.Errors;

        _variants.Add(variantResult.Value);

        return variantResult.Value;
    }

    public Result<Updated> UpdateVariant(Guid variantId, string? size, string? color, int stockQuantity)
    {
        Variant? variant = _variants.FirstOrDefault(variant => variant.Id == variantId);

        if (variant is null)
            return ProductErrors.VariantNotFound;

        return variant.Update(size, color, stockQuantity);
    }

    public Result<Deleted> RemoveVariant(Guid variantId)
    {
        Variant? variant = _variants.FirstOrDefault(variant => variant.Id == variantId);

        if (variant is null)
            return ProductErrors.VariantNotFound;

        _variants.Remove(variant);

        return Result.Deleted;
    }

    public Result<Image> AddImage(string? imageUrl, bool isMain = false)
    {
        Result<Image> imageResult = Image.Create(Id, imageUrl, isMain);

        if (imageResult.IsError)
            return imageResult.Errors;

        if (imageResult.Value.IsMain)
            ClearMainImage();

        _images?.Add(imageResult.Value);

        return imageResult.Value;
    }

    public Result<Updated> SetMainImage(Guid imageId)
    {
        Image? image = _images?.FirstOrDefault(image => image.Id == imageId);

        if (image is null)
            return ProductErrors.ImageNotFound;

        ClearMainImage();

        return image.MarkAsMain();
    }

    public Result<Deleted> RemoveImage(Guid imageId)
    {
        Image? image = _images?.FirstOrDefault(image => image.Id == imageId);

        if (image is null)
            return ProductErrors.ImageNotFound;

        _images?.Remove(image);

        return Result.Deleted;
    }

    private void ClearMainImage()
    {
        foreach (Image image in _images!.Where(image => image.IsMain))
            image.UnmarkAsMain();
    }

    private static Error? Validate(string? name, string? description, decimal basePrice, decimal? discount, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ProductErrors.NameRequired;

        //if (string.IsNullOrWhiteSpace(description))
        //    return ProductErrors.DescriptionRequired;

        if (basePrice <= 0)
            return ProductErrors.InvalidBasePrice;

        if (discount < 0)
            return ProductErrors.InvalidDiscount;

        if (categoryId == Guid.Empty)
            return ProductErrors.CategoryIdRequired;

        return null;
    }
}
