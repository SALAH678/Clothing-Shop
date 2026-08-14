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
    public static Error InvalidFile => Error.Validation(code: "Image_InvalidFile", description: "The provided file is invalid or empty.");
    public static Error UnsupportedFormat => Error.Validation(code: "Image_UnsupportedFormat", description: "Only .jpg, .jpeg, .png, and .webp files are supported.");
    public static Error FileTooLarge => Error.Validation(code: "Image_FileTooLarge", description: "Image must be smaller than 5MB.");
    public static Error SaveFailed => Error.Failure(code: "Image_SaveFailed", description: "Failed to save the image.");
    public static Error NotFound => Error.NotFound(code: "Image_NotFound", description: "The image could not be found.");
    public static Error DeleteFailed => Error.Failure(code: "Image_DeleteFailed", description: "Failed to delete the image.");
}
