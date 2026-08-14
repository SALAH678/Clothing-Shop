using Domain.Common.Results;

namespace Domain.Common.ValueObjects.Address;

public class Address
{
    public string Street { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string Wilaya { get; private set; } = null!;

    private Address(string street, string city, string wilaya)
    {
        Street = street;
        City = city;
        Wilaya = wilaya;
    }

    public static Result<Address> Create(string? street, string? city, string? wilaya)
    {
        if (string.IsNullOrWhiteSpace(street))
            return AddressErrors.StreetRequired;

        if (string.IsNullOrWhiteSpace(city))
            return AddressErrors.CityRequired;

        if (string.IsNullOrWhiteSpace(wilaya))
            return AddressErrors.WilayaRequired;

        string preparedStreet = street.Trim();
        string preparedCity = city.Trim();
        string preparedWilaya = wilaya.Trim();

        if (preparedStreet.Length < 5)
            return AddressErrors.StreetTooShort;

        if (preparedStreet.Length > 200)
            return AddressErrors.StreetTooLong;

        if (preparedCity.Length < 2)
            return AddressErrors.CityTooShort;

        if (preparedCity.Length > 100)
            return AddressErrors.CityTooLong;

        if (preparedWilaya.Length < 2)
            return AddressErrors.WilayaTooShort;

        if (preparedWilaya.Length > 100)
            return AddressErrors.WilayaTooLong;

        return new Address(preparedStreet, preparedCity, preparedWilaya);
    }
}