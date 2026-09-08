using Domain.Common.Results;

namespace Domain.Users.RefreshTokens
{
    public static class RefreshTokenErrors
    {
        public static Error RefreshTokenRequired => Error.Validation(code: "Refresh_Token_Required", description: "Refresh token is required.");
        public static Error UserIdRequired => Error.Validation(code: "Refresh_Token_User_Id_Required", description: "User id is required.");
        public static Error ValueRequired => Error.Validation(code: "Refresh_Token_Value_Required", description: "Refresh token value is required.");
        public static Error ExpiresAtUtcMustBeInFuture => Error.Validation(code: "Refresh_Token_Expiration_Invalid", description: "Refresh token expiration must be in the future.");
        public static Error RefreshTokenAlreadyRevoked => Error.Validation(code: "Refresh_Token_Already_Revoked", description: "Refresh token is already revoked.");
    }
}
