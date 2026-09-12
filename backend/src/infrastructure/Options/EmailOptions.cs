using System.ComponentModel.DataAnnotations;

namespace infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    [Required(ErrorMessage = "SMTP host is required.")]
    public string SmtpHost { get; init; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP port must be a valid port number between 1 and 65535.")]
    public int SmtpPort { get; init; }

    public bool EnableSsl { get; init; }

    [Required(ErrorMessage = "SMTP username is required.")]
    public string Username { get; init; } = string.Empty;

    [Required(ErrorMessage = "SMTP password is required.")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "From email address is required.")]
    [EmailAddress(ErrorMessage = "Invalid From email address format.")]
    public string FromEmail { get; init; } = string.Empty;

    [Required(ErrorMessage = "From name is required.")]
    public string FromName { get; init; } = string.Empty;
}
