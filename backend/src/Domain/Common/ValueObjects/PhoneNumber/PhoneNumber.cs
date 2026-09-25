using Domain.Common.Results;
using System.Text.RegularExpressions;

namespace Domain.Common.ValueObjects.PhoneNumber;

public class PhoneNumber
{
    public string Value { get; private set; } = null!;

    private PhoneNumber(string value)
    {
        Value = value;
    }

    private static readonly Regex PhoneNumberRegex = new(@"^0[5-7][0-9]{8}$", RegexOptions.Compiled);

    public static Result<PhoneNumber> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return PhoneNumberErrors.PhoneNumberRequired;

        string preparedPhoneNumber = value.Trim();

        //BR
        if (!PhoneNumberRegex.IsMatch(preparedPhoneNumber))
            return PhoneNumberErrors.ShouldBeAlgerianNumber;

        return new PhoneNumber(preparedPhoneNumber);
    }
}
