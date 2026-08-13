
using Domain.Common.Results;

namespace Domain.Common.ValueObjects.Email;

public static class EmailErrors
{
    public static Error EmailRequired => Error.Validation(code: "Email_Value_Required", description: "Email is required.");
    public static Error InvalidEmail => Error.Validation(code: "Email_Invalid", description: "Invalid email format.");
    public static Error SendFailed => Error.Failure(
     code: "Email_SendFailed",
     description: "Failed to send email. Please try again later.");
}
