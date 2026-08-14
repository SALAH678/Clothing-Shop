using Domain.Common;
using Domain.Common.Results;

namespace Domain.Products.Variants;

public class Variant : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public string Size { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public int StockQuantity { get; private set; }

    public Product Product { get; private set; } = null!;

    protected Variant()
    {
    }

    protected Variant(Guid productId, string size, string color, int stockQuantity)
        : base(Guid.Empty)
    {
        ProductId = productId;
        Size = size;
        Color = color;
        StockQuantity = stockQuantity;
    }

    public static Result<Variant> Create(Guid productId, string? size, string? color, int stockQuantity)
    {
        Error? error = Validate(productId, size, color, stockQuantity);

        if (error is not null)
            return error.Value;

        return new Variant(productId, size!.Trim(), color!.Trim(), stockQuantity);
    }

    public Result<Updated> Update(string? size, string? color, int stockQuantity)
    {
        Error? error = Validate(ProductId, size, color, stockQuantity);

        if (error is not null)
            return error.Value;

        Size = size!.Trim();
        Color = color!.Trim();
        StockQuantity = stockQuantity;

        return Result.Updated;
    }

    public Result<Updated> IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            return VariantErrors.InvalidStockQuantity;

        StockQuantity += quantity;

        return Result.Updated;
    }

    public Result<Updated> DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            return VariantErrors.InvalidStockQuantity;

        if (quantity > StockQuantity)
            return VariantErrors.InsufficientStock;

        StockQuantity -= quantity;

        return Result.Updated;
    }

    private static Error? Validate(Guid productId, string? size, string? color, int stockQuantity)
    {
        if (productId == Guid.Empty)
            return VariantErrors.ProductIdRequired;

        if (string.IsNullOrWhiteSpace(size))
            return VariantErrors.SizeRequired;

        if (string.IsNullOrWhiteSpace(color))
            return VariantErrors.ColorRequired;

        if (stockQuantity < 0)
            return VariantErrors.InvalidStockQuantity;

        return null;
    }
}
