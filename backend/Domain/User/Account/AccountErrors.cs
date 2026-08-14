using Domain.Common.Results;

namespace Domain.Users.Accounts
{
    public static class AccountErrors
    {
        public static Error AccountRequired => Error.Validation(code: "Account_Required", description: "Account is required.");
        public static Error UserIdRequired => Error.Validation(code: "Account_User_Id_Required", description: "User id is required.");
        public static Error ProviderRequired => Error.Validation(code: "Account_Provider_Required", description: "Provider is required.");
        public static Error ProviderAccountIdRequired => Error.Validation(code: "Provider_Account_Id_Required", description: "Provider account id is required.");
        public static Error PasswordRequired => Error.Validation(code: "Password_Required", description: "Password is required.");
    }
}
