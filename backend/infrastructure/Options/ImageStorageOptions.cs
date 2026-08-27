namespace infrastructure.Options;

public sealed class ImageStorageOptions
{
    public const string SectionName = "ImageStorage";

    public string BasePath { get; init; } = string.Empty;

    public string BaseUrl { get; init; } = string.Empty;
}
