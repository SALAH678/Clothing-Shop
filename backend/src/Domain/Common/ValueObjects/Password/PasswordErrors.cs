using Domain.Common.Results;

namespace Domain.Common.ValueObjects.Password;

public static class PasswordErrors
{
    public static Error PasswordRequired => Error.Validation(code: "Password_Required", description: "Password Is Required.");
    public static Error ShortPassword => Error.Validation(code: "Short_Password", description: "Password must be at least 6 characters long.");
    public static Error WeakPassword => Error.Validation(code: "Invalid_Password", description: "Password must contain at least one uppercase and lowercase letter, and one number and one special character.");
}
