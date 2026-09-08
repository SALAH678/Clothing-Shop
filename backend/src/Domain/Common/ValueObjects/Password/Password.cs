using Domain.Common.Results;
using System.Text.RegularExpressions;

namespace Domain.Common.ValueObjects.Password;

public class Password
{
    public string Value { get; private set; } = null!;

    private Password(string value)
    {
        Value = value;
    }

    private static readonly Regex PasswordRegex = new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{6,}$", RegexOptions.Compiled);

    public static Result<Password> Create(string? password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordErrors.PasswordRequired;

        string preparedPassword = password.Trim();

        if (preparedPassword.Length < 6)
            return PasswordErrors.ShortPassword;

        if (!PasswordRegex.IsMatch(preparedPassword))
            return PasswordErrors.WeakPassword;

        return new Password(preparedPassword);
    }
}
