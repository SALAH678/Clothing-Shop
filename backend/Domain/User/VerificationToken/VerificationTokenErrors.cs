using Domain.Common.Results;

namespace Domain.Users.VerificationTokens
{
    public static class VerificationTokenErrors
    {
        public static Error VerificationTokenRequired => Error.Validation(code: "Verification_Token_Required", description: "Verification token is required.");
        public static Error UserIdRequired => Error.Validation(code: "Verification_Token_User_Id_Required", description: "User id is required.");
        public static Error CodeRequired => Error.Validation(code: "Verification_Token_Code_Required", description: "Verification token code is required.");
        public static Error ExpiresAtUtcMustBeInFuture => Error.Validation(code: "Verification_Token_Expiration_Invalid", description: "Verification token expiration must be in the future.");
        public static Error TypeRequired => Error.Validation(code: "Verification_Token_Type_Required", description: "Verification token type is required.");
        public static Error InvalidType => Error.Validation(code: "Verification_Token_Type_Invalid", description: "Verification token type is invalid.");
        public static Error VerificationTokenAlreadyUsed => Error.Validation(code: "Verification_Token_Already_Used", description: "Verification token is already used.");
        public static Error CodeAlreadyUsed => Error.Conflict(code: "EmailVerificationToken.CodeAlreadyUsed", description: "This verification code has already been used.");
        public static Error CodeExpired =>Error.Conflict( code: "EmailVerificationToken.CodeExpired", description: "This verification code has expired.");
        public static Error InvalidOrExpiredCode => Error.NotFound( code: "EmailVerificationToken.InvalidOrExpiredCode", description: "The verification code is invalid or expired.");
    }
}
