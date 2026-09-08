using Domain.Common.Results;

namespace Domain.Carts.CartItems;

public static class CartItemErrors
{
    public static Error CartItemRequired => Error.Validation(code: "Cart_Item_Required", description: "Cart item is required.");
    public static Error CartIdRequired => Error.Validation(code: "Cart_Item_Cart_Id_Required", description: "Cart id is required.");
    public static Error VariantIdRequired => Error.Validation(code: "Cart_Item_Variant_Id_Required", description: "Variant id is required.");
    public static Error InvalidQuantity => Error.Validation(code: "Cart_Item_Invalid_Quantity", description: "Quantity must be greater than zero.");
}
