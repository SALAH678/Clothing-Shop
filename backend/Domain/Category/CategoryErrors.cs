using Domain.Common.Results;

namespace Domain.Categories;

public static class CategoryErrors
{
    public static Error CategoryRequired => Error.Validation(code: "Category_Required", description: "Category is required.");
    public static Error CategoryNameRequired => Error.Validation(code: "Category_Name_Required", description: "Category name is required.");
    public static Error ImageUrlRequired => Error.Validation(code: "Category_Image_Url_Required", description: "Category image URL is required.");
    public static Error InvalidParentCategoryId => Error.Validation(code: "Category_Invalid_Parent_Category_Id", description: "Parent category id is invalid.");
    public static Error CannotBeOwnParent => Error.Validation(code: "Category_Cannot_Be_Own_Parent", description: "Category cannot be its own parent.");
    public static Error ChildCategoryNotFound => Error.NotFound(code: "Category_Child_Not_Found", description: "Child category was not found.");
    public static Error ImageUrlTooLong =>
           Error.Validation(code: "Image_ImageUrl_TooLong", description: "Image URL cannot exceed 500 characters.");
    public static Error InvalidImageUrl =>
            Error.Validation(code: "Image_Invalid_ImageUrl", description: "Image URL must be a valid URL.");
}
