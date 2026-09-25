
using Domain.Common.Results;

namespace Domain.Common.ValueObjects.PhoneNumber;

public static class PhoneNumberErrors
{
    public static Error PhoneNumberRequired => Error.Validation("PhoneNumber_Required", "Phone number is required.");
    public static Error InvalidPhoneNumber => Error.Validation("PhoneNumber_Invalid", "Phone number is invalid.");
    public static Error ShouldBeAlgerianNumber => Error.Validation("PhoneNumber_ShouldBeAlgerianNumber", "Phone number should be a valid Algerian mobile number.");
}

