using Domain.Carts.CartItems;
using Domain.Common;
using Domain.Common.Results;
using Domain.Users;

namespace Domain.Carts;

public class Cart : AuditableEntity
{
    public Guid UserId { get; private set; }

    private readonly List<CartItem>? _items = [];

    public IReadOnlyCollection<CartItem>? Items => _items?.AsReadOnly();

    public User User { get; private set; } = null!;

    protected Cart()
    {
    }

    protected Cart(Guid userId)
        : base(Guid.Empty)
    {
        UserId = userId;
    }

    public static Result<Cart> Create(Guid userId)
    {
        if (userId == Guid.Empty)
            return CartErrors.UserIdRequired;

        return new Cart(userId);
    }

    public Result<CartItem> AddItem(Guid variantId, int quantity)
    {
        CartItem? existingItem = _items?.FirstOrDefault(item => item.VariantId == variantId);

        if (existingItem is not null)
        {
            Result<Updated> updateResult = existingItem.IncreaseQuantity(quantity);

            if (updateResult.IsError)
                return updateResult.Errors;

            return existingItem;
        }

        Result<CartItem> cartItemResult = CartItem.Create(Id, variantId, quantity);

        if (cartItemResult.IsError)
            return cartItemResult.Errors;

        _items?.Add(cartItemResult.Value);

        return cartItemResult.Value;
    }

    public Result<Updated> UpdateItemQuantity(Guid variantId, int quantity)
    {
        CartItem? item = _items?.FirstOrDefault(item => item.VariantId == variantId);

        if (item is null)
            return CartErrors.ItemNotFound;

        return item.UpdateQuantity(quantity);
    }

    public Result<Deleted> RemoveItem(Guid variantId)
    {
        CartItem? item = _items?.FirstOrDefault(item => item.VariantId == variantId);

        if (item is null)
            return CartErrors.ItemNotFound;

        _items?.Remove(item);

        return Result.Deleted;
    }

    public Result<Deleted> Clear()
    {
        if (_items?.Count == 0)
            return CartErrors.CartIsEmpty;

        _items?.Clear();

        return Result.Deleted;
    }
}
