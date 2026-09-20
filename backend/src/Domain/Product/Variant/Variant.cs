using Domain.Carts.CartItems;
using Domain.Common;
using Domain.Common.Results;
using Domain.Purchases.PurchaseItems;

namespace Domain.Products.Variants;

public class Variant : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public string Size { get; private set; } = null!;
    public string Color { get; private set; } = null!;
    public int StockQuantity { get; private set; }

    private readonly List<PurchaseItem>? _purchaseItems = [];
    private readonly List<CartItem>? _cartItems = [];

    public IReadOnlyCollection<PurchaseItem>? PurchaseItems => _purchaseItems?.AsReadOnly();
    public IReadOnlyCollection<CartItem>? CartItems => _cartItems?.AsReadOnly();

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

        //BR
        if(!ValidSizes.Contains(size!.Trim().ToLowerInvariant()))
            return VariantErrors.InvalidSize;

        return new Variant(productId, size!.Trim(), color!.Trim(), stockQuantity);
    }

    private static readonly HashSet<string> ValidSizes = new() { "s", "m", "l", "xl", "xxl", "xxxl" };

    public Result<Updated> Update(string? size, string? color, int stockQuantity)
    {
        Error? error = Validate(ProductId, size, color, stockQuantity);

        if (error is not null)
            return error.Value;

        //BR
        if (!ValidSizes.Contains(size!.Trim().ToLowerInvariant()))
            return VariantErrors.InvalidSize;

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
