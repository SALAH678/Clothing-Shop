using Domain.Common.Results;

namespace Domain.Products.Variants;

public static class VariantErrors
{
    public static Error VariantRequired => Error.Validation(code: "Variant_Required", description: "Variant is required.");
    public static Error ProductIdRequired => Error.Validation(code: "Variant_Product_Id_Required", description: "Product id is required.");
    public static Error SizeRequired => Error.Validation(code: "Variant_Size_Required", description: "Variant size is required.");
    public static Error ColorRequired => Error.Validation(code: "Variant_Color_Required", description: "Variant color is required.");
    public static Error InvalidStockQuantity => Error.Validation(code: "Variant_Invalid_Stock_Quantity", description: "Stock quantity cannot be negative.");
    public static Error InsufficientStock => Error.Validation(code: "Variant_Insufficient_Stock", description: "Stock quantity is not enough.");
}
