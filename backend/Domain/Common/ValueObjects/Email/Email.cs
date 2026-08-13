
using Domain.Common.Results;
using System.Text.RegularExpressions;

namespace Domain.Common.ValueObjects.Email;

public class Email
{
    public string Value { get; private set; } = null!;

    private Email(string value)
    {
        Value = value;
    }

    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return EmailErrors.EmailRequired;

        string preparedEmail = email.Trim().ToLowerInvariant();

        if (!EmailRegex.IsMatch(preparedEmail))
            return EmailErrors.InvalidEmail;

        return new Email(preparedEmail);
    }
}


