using Domain.Common;
using Domain.Common.Results;
using Domain.Products.Variants;

namespace Domain.Purchases.PurchaseItems;

public class PurchaseItem : AuditableEntity
{
    public Guid PurchaseId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    public Purchase Purchase { get; private set; } = null!;
    public Variant Variant { get; private set; } = null!;

    protected PurchaseItem()
    {
    }

    protected PurchaseItem(Guid purchaseId, Guid variantId, int quantity, decimal unitPrice)
        : base(Guid.Empty)
    {
        PurchaseId = purchaseId;
        VariantId = variantId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    public static Result<PurchaseItem> Create(Guid purchaseId, Guid variantId, int quantity, decimal unitPrice)
    {
        Error? error = Validate(purchaseId, variantId, quantity, unitPrice);

        if (error is not null)
            return error.Value;

        return new PurchaseItem(purchaseId, variantId, quantity, unitPrice);
    }

    public void AttachVariant(Variant variant)
    {
        Variant = variant;
    }

    public Result<Updated> UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            return PurchaseItemErrors.InvalidQuantity;

        Quantity = quantity;

        return Result.Updated;
    }

    public Result<Updated> IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            return PurchaseItemErrors.InvalidQuantity;

        Quantity += quantity;

        return Result.Updated;
    }

    public Result<Updated> UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice < 0)
            return PurchaseItemErrors.InvalidUnitPrice;

        UnitPrice = unitPrice;

        return Result.Updated;
    }

    private static Error? Validate(Guid purchaseId, Guid variantId, int quantity, decimal unitPrice)
    {
        if (purchaseId == Guid.Empty)
            return PurchaseItemErrors.PurchaseIdRequired;

        if (variantId == Guid.Empty)
            return PurchaseItemErrors.VariantIdRequired;

        if (quantity <= 0)
            return PurchaseItemErrors.InvalidQuantity;

        if (unitPrice < 0)
            return PurchaseItemErrors.InvalidUnitPrice;

        return null;
    }
}
