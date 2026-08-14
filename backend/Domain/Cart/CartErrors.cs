using Domain.Common.Results;

namespace Domain.Carts;

public static class CartErrors
{
    public static Error CartRequired => Error.Validation(code: "Cart_Required", description: "Cart is required.");
    public static Error UserIdRequired => Error.Validation(code: "Cart_User_Id_Required", description: "User id is required.");
    public static Error ItemNotFound => Error.NotFound(code: "Cart_Item_Not_Found", description: "Cart item was not found.");
    public static Error CartIsEmpty => Error.Validation(code: "Cart_Is_Empty", description: "Cart is empty.");
}
