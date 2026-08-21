using Domain.Common.Results;

namespace Domain.Products.Images;

public static class ImageErrors
{
    public static Error ImageRequired => Error.Validation(code: "Image_Required", description: "Image is required.");
    public static Error ProductIdRequired => Error.Validation(code: "Image_Product_Id_Required", description: "Product id is required.");
    public static Error ImageUrlRequired => Error.Validation(code: "Image_Url_Required", description: "Image URL is required.");
    public static Error ImageUrlTooLong =>
           Error.Validation(code: "Image_ImageUrl_TooLong", description: "Image URL cannot exceed 500 characters.");
    public static Error InvalidImageUrl =>
            Error.Validation(code: "Image_Invalid_ImageUrl", description: "Image URL must be a valid URL.");
}
