using Domain.Common.Results;

namespace Application.Common.Errors;

public static class ApplicationErrors
{
    public static Error EmailAlreadyExists => Error.Conflict(
            code: "Email_Already_Exists",
            description: "The provided email address is already in use.");
    public static Error RegistrationFailed => Error.Failure(
    code: "Registration_Failed",
    description: "User registration failed.");
    public static Error InvalidCredentials => Error.Unauthorized(
            code: "Invalid_Credentials",
            description: "The provided credentials are invalid.");
    public static Error EmailNotVerified => Error.Unauthorized(
            code: "Authentication.EmailNotVerified",
            description: "Please verify your email address before signing in.");
    public static Error UserNotFound => Error.NotFound(
    code: "User_Not_Found",
    description: "The provided user was not found.");
    public static Error LoginFailed => Error.Failure(
    code: "Login_Failed",
    description: "User login failed.");
    public static Error InvalidRefreshRequest => Error.Unauthorized(
    code: "Authentication_InvalidRefreshRequest",
    description: "The provided refresh request is invalid.");
    public static Error RefreshTokenNotFound => Error.NotFound(
    code: "Authentication_RefreshTokenNotFound",
    description: "The provided refresh token was not found.");
    public static Error RefreshTokenIsRevoked => Error.Unauthorized(
    code: "Authentication_RefreshTokenIsRevoked",
    description: "Refresh token has been revoked.");
    public static Error RefreshTokenExpired => Error.Unauthorized(
    code: "Authentication_RefreshTokenExpired",
    description: "Refresh token has expired.");
    public static Error RefreshFailed => Error.Failure(
    code: "Authentication_RefreshFailed",
    description: "Token refresh failed.");
    public static Error LogOutFailed => Error.Failure(
    code: "Authentication_LogOutFailed",
    description: "Log out failed.");
    public static Error EmailAlreadyVerified => Error.Conflict(
    code: "Authentication.EmailAlreadyVerified",
    description: "This email address has already been verified.");
    public static Error VerificationFailed => Error.Failure(
    code: "Authentication_VerificationFailed",
    description: "Email verification failed.");
    public static Error VerificationTokenNotFound => Error.NotFound(
        code: "Authentication_VerificationTokenNotFound",
        description: "VerificationToken are not found");
    public static Error InvalidVerificationRequest => Error.Validation(
        code: "Authentication_InvalidVerificationRequest",
        description: "The provided verification request is invalid.");
    public static Error ResendVerificationCodeFailed => Error.Failure(
        code: "Authentication_ResendVerificationCodeFailed",
        description: "Resend verification code failed.");
    public static Error ForgotPasswordFailed => Error.Failure(
        code: "Authentication_ForgotPasswordFailed",
        description: "Forgot password request failed.");
    public static Error InvalidPasswordResetRequest => Error.Validation(
        code: "Authentication_InvalidPasswordResetRequest",
        description: "The provided password reset request is invalid.");
    public static Error PasswordResetFailed => Error.Failure(
        code: "Authentication_PasswordResetFailed",
        description: "Password reset failed.");
    public static Error CategoryCreationFailed => Error.Failure(
        code: "Category_Creation_Failed",
        description: "Category creation failed.");
    public static Error CategoryDeletFailed => Error.Failure(
        code: "Category_Delete_Failed",
        description: "Category delete failed.");
    public static Error CategoryUpdateFailed => Error.Failure(
        code: "Category_Update_Failed",
        description: "Category update failed.");
    public static Error SubCategoryNotFound => Error.NotFound(
        code: "SubCategory_Not_Found",
        description: "One or more subcategories were not found.");
    public static Error CircularReferenceDetected => Error.Conflict(
        code: "Circular_Reference_Detected",
        description: "A circular reference was detected in the category hierarchy.");
    public static Error AssignFailed => Error.Failure(
        code: "Assign_Failed",
        description: "Failed to assign subcategories to the category.");
    public static Error UpdateUserFailed => Error.Failure(
    code: "Update_User_Failed",
    description: "Failed to update user information.");
    public static Error CartOperationFailed => Error.Failure(
        code: "Cart_Operation_Failed",
        description: "Failed to update the cart.");
    public static Error CartAlreadyExists => Error.Conflict(
        code: "Cart_Already_Exists",
        description: "A cart already exists for this user.");
    public static Error CartNotExist => Error.NotFound(
        code: "Cart_Not_Exist",
        description: "The cart does not exist for this user.");
    public static Error ProductsNotExists => Error.NotFound(
        code: "Products_Not_Exists",
        description: "products does not exist");
    public static Error InvalidGoogleId => Error.NotFound(
        code: "Invalid_Google_Id",
        description: "Invalid Google ID token.");
    public static Error OAuthFailed => Error.Failure(
        code: "OAuth_Failed",
        description: "OAuth authentication failed.");
    public static Error PaymentNotFound => Error.NotFound(
        code: "Payment_Not_Found",
        description: "The payment was not found.");
    public static Error DatabaseSaveFailed => Error.Failure(
        code: "Database_Save_Failed",
        description: "Failed to save changes to the database.");
    public static Error AccountAlreadyExists => Error.Conflict(
        code: "Account_Already_Exists",
        description: "An account with this provider already exists.");
    public static Error UserAlreadyExists => Error.Conflict(
        code: "User_Already_Exists",
        description: "A user with this email already exists.");
    public static Error VariantNotFound => Error.NotFound(
        code: "Variant_Not_Found",
        description: "The specified variant was not found.");
}
