using Domain.Common.Results;

namespace Domain.Products;

public static class ProductErrors
{
    public static Error ProductRequired => Error.Validation(code: "Product_Required", description: "Product is required.");
    public static Error NameRequired => Error.Validation(code: "Product_Name_Required", description: "Product name is required.");
    public static Error DescriptionRequired => Error.Validation(code: "Product_Description_Required", description: "Product description is required.");
    public static Error InvalidBasePrice => Error.Validation(code: "Product_Invalid_Base_Price", description: "Base price must be greater than zero.");
    public static Error InvalidDiscount => Error.Validation(code: "Product_Invalid_Discount", description: "Discount must be between 0 and 100.");
    public static Error CategoryIdRequired => Error.Validation(code: "Product_Category_Id_Required", description: "Category id is required.");
    public static Error VariantAlreadyExists => Error.Conflict(code: "Product_Variant_Already_Exists", description: "Product variant already exists.");
    public static Error VariantNotFound => Error.NotFound(code: "Product_Variant_Not_Found", description: "Product variant was not found.");
    public static Error ImageNotFound => Error.NotFound(code: "Product_Image_Not_Found", description: "Product image was not found.");
    public static Error DiscountShouldNotExceedOneMilyen => Error.Validation(code: "Product_Discount_Should_Not_Exceed_One_Milyen", description: "Discount should not exceed one milyen.");
}
