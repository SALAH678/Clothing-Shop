using Domain.Common.Results;

namespace Domain.Common.ValueObjects.Address;

public static class AddressErrors
{
    public static Error StreetRequired => Error.Validation(
        code: "Address_Street_Required",
        description: "Street address is required.");

    public static Error StreetTooShort => Error.Validation(
        code: "Address_Street_TooShort",
        description: "Street address must be at least 5 characters long.");

    public static Error StreetTooLong => Error.Validation(
        code: "Address_Street_TooLong",
        description: "Street address cannot exceed 200 characters.");

    public static Error CityRequired => Error.Validation(
        code: "Address_City_Required",
        description: "City is required.");

    public static Error CityTooShort => Error.Validation(
        code: "Address_City_TooShort",
        description: "City must be at least 2 characters long.");

    public static Error CityTooLong => Error.Validation(
        code: "Address_City_TooLong",
        description: "City cannot exceed 100 characters.");

    public static Error StateRequired => Error.Validation(
        code: "Address_State_Required",
        description: "State/Province is required.");

    public static Error StateTooShort => Error.Validation(
        code: "Address_State_TooShort",
        description: "State/Province must be at least 2 characters long.");

    public static Error StateTooLong => Error.Validation(
        code: "Address_State_TooLong",
        description: "State/Province cannot exceed 100 characters.");

    public static Error PostalCodeRequired => Error.Validation(
        code: "Address_PostalCode_Required",
        description: "Postal code is required.");

    public static Error PostalCodeTooShort => Error.Validation(
        code: "Address_PostalCode_TooShort",
        description: "Postal code must be at least 2 characters long.");

    public static Error PostalCodeTooLong => Error.Validation(
        code: "Address_PostalCode_TooLong",
        description: "Postal code cannot exceed 20 characters.");

    public static Error CountryRequired => Error.Validation(
        code: "Address_Country_Required",
        description: "Country is required.");

    public static Error CountryTooShort => Error.Validation(
        code: "Address_Country_TooShort",
        description: "Country must be at least 2 characters long.");

    public static Error CountryTooLong => Error.Validation(
        code: "Address_Country_TooLong",
        description: "Country cannot exceed 100 characters.");
}
