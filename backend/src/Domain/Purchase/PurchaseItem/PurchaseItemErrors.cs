using Domain.Common.Results;

namespace Domain.Purchases.PurchaseItems;

public static class PurchaseItemErrors
{
    public static Error PurchaseItemRequired => Error.Validation(code: "Purchase_Item_Required", description: "Purchase item is required.");
    public static Error PurchaseIdRequired => Error.Validation(code: "Purchase_Item_Purchase_Id_Required", description: "Purchase id is required.");
    public static Error VariantIdRequired => Error.Validation(code: "Purchase_Item_Variant_Id_Required", description: "Variant id is required.");
    public static Error InvalidQuantity => Error.Validation(code: "Purchase_Item_Invalid_Quantity", description: "Quantity must be greater than zero.");
    public static Error InvalidUnitPrice => Error.Validation(code: "Purchase_Item_Invalid_Unit_Price", description: "Unit price cannot be negative.");
}
