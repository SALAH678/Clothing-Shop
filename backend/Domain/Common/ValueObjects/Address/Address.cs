using Domain.Common.Results;

namespace Domain.Common.ValueObjects.Address;

public class Address
{
    public string Street { get; private set; } = null!;
    public string? Line2 { get; private set; }
    public string City { get; private set; } = null!;
    public string State { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    private Address(string street, string? line2, string city, string state, string postalCode, string country)
    {
        Street = street;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public static Result<Address> Create(string? street, string? line2, string? city, string? state, string? postalCode, string? country)
    {
        if (string.IsNullOrWhiteSpace(street))
            return AddressErrors.StreetRequired;

        if (string.IsNullOrWhiteSpace(city))
            return AddressErrors.CityRequired;

        if (string.IsNullOrWhiteSpace(state))
            return AddressErrors.StateRequired;

        if (string.IsNullOrWhiteSpace(postalCode))
            return AddressErrors.PostalCodeRequired;

        if (string.IsNullOrWhiteSpace(country))
            return AddressErrors.CountryRequired;

        string preparedStreet = street.Trim();
        string? preparedLine2 = string.IsNullOrWhiteSpace(line2) ? null : line2.Trim();
        string preparedCity = city.Trim();
        string preparedState = state.Trim();
        string preparedPostalCode = postalCode.Trim();
        string preparedCountry = country.Trim();

        if (preparedStreet.Length < 5)
            return AddressErrors.StreetTooShort;

        if (preparedStreet.Length > 200)
            return AddressErrors.StreetTooLong;

        if (preparedCity.Length < 2)
            return AddressErrors.CityTooShort;

        if (preparedCity.Length > 100)
            return AddressErrors.CityTooLong;

        if (preparedState.Length < 2)
            return AddressErrors.StateTooShort;

        if (preparedState.Length > 100)
            return AddressErrors.StateTooLong;

        if (preparedPostalCode.Length < 2)
            return AddressErrors.PostalCodeTooShort;

        if (preparedPostalCode.Length > 20)
            return AddressErrors.PostalCodeTooLong;

        if (preparedCountry.Length < 2)
            return AddressErrors.CountryTooShort;

        if (preparedCountry.Length > 100)
            return AddressErrors.CountryTooLong;

        return new Address(preparedStreet, preparedLine2, preparedCity, preparedState, preparedPostalCode, preparedCountry);
    }
}
