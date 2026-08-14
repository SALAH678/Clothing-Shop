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

    public static Error WilayaRequired => Error.Validation(
        code: "Address_Wilaya_Required",
        description: "Wilaya is required.");

    public static Error WilayaTooShort => Error.Validation(
        code: "Address_Wilaya_TooShort",
        description: "Wilaya must be at least 2 characters long.");

    public static Error WilayaTooLong => Error.Validation(
        code: "Address_Wilaya_TooLong",
        description: "Wilaya cannot exceed 100 characters.");
}