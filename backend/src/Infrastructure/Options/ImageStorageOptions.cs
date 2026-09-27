using System.ComponentModel.DataAnnotations;

namespace infrastructure.Options;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    [Required(ErrorMessage = "Image storage base path is required.")]
    public string BasePath { get; init; } = string.Empty;

    [Required(ErrorMessage = "Image storage base URL is required.")]
    public string BaseUrl { get; init; } = string.Empty;
}
