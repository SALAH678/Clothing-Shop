using System.ComponentModel.DataAnnotations;

namespace infrastructure.Options;

public sealed class ChargilyOptions
{
    public const string SectionName = "Chargily";

    [Required(ErrorMessage = "Chargily API Secret Key is required.")]
    public string ApiSecretKey { get; init; } = string.Empty;

    public bool IsLiveMode { get; init; } = false;

    [Required, Url]
    public string WebhookEndpointUrl { get; init; } = string.Empty;

    [Required, Url]
    public string SuccessRedirectBaseUrl { get; init; } = string.Empty;

    [Required, Url]
    public string FailureRedirectBaseUrl { get; init; } = string.Empty;
}
