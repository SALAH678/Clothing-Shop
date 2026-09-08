namespace infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; }

    public bool EnableSsl { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;
}
