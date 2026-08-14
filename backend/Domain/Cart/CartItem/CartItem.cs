using Domain.Common;
using Domain.Common.Results;
using Domain.Products.Variants;

namespace Domain.Carts.CartItems;

public class CartItem : AuditableEntity
{
    public Guid CartId { get; private set; }
    public Guid VariantId { get; private set; }
    public int Quantity { get; private set; }

    public Cart Cart { get; private set; } = null!;
    public Variant Variant { get; private set; } = null!;

    protected CartItem()
    {
    }

    protected CartItem(Guid cartId, Guid variantId, int quantity)
        : base(Guid.Empty)
    {
        CartId = cartId;
        VariantId = variantId;
        Quantity = quantity;
    }

    public static Result<CartItem> Create(Guid cartId, Guid variantId, int quantity)
    {
        Error? error = Validate(cartId, variantId, quantity);

        if (error is not null)
            return error.Value;

        return new CartItem(cartId, variantId, quantity);
    }

    public Result<Updated> UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            return CartItemErrors.InvalidQuantity;

        Quantity = quantity;

        return Result.Updated;
    }

    public Result<Updated> IncreaseQuantity(int quantity)
    {
        if (quantity <= 0)
            return CartItemErrors.InvalidQuantity;

        Quantity += quantity;

        return Result.Updated;
    }

    private static Error? Validate(Guid cartId, Guid variantId, int quantity)
    {
        if (cartId == Guid.Empty)
            return CartItemErrors.CartIdRequired;

        if (variantId == Guid.Empty)
            return CartItemErrors.VariantIdRequired;

        if (quantity <= 0)
            return CartItemErrors.InvalidQuantity;

        return null;
    }
}
