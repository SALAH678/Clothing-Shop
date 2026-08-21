using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Configuration;

namespace infrastructure.Services;

public sealed class ImageService : IImageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    private readonly string _rootPath;
    private readonly string _rootUrl;

    public ImageService(IConfiguration configuration)
    {
        _rootPath = configuration["ImageStorage:BasePath"]
            ?? throw new InvalidOperationException("ImageStorage:BasePath is not configured.");
        _rootUrl = configuration["ImageStorage:BaseUrl"]
            ?? throw new InvalidOperationException("ImageStorage:BaseUrl is not configured.");
    }

    public async Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        if (content is null || content.Length == 0)
            throw new ArgumentException("The provided image file is invalid or empty.", nameof(content));

        if (content.Length > MaxFileSizeBytes)
            throw new InvalidDataException("Image must be smaller than 5MB.");

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidDataException("Only .jpg, .jpeg, .png, and .webp files are supported.");

        var folderPath = Path.Combine(_rootPath, folder);

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var fullPath = Path.Combine(folderPath, uniqueFileName);

        try
        {
            await using var fileStream = new FileStream(fullPath, FileMode.Create);
            await content.CopyToAsync(fileStream, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new IOException("Failed to save the image.", exception);
        }

        return $"{_rootUrl}/{folder}/{uniqueFileName}";
    }

    public async Task<string> UpdateAsync(string? existingImageUrl, Stream newContent, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var imageUrl = await SaveAsync(newContent, fileName, folder, cancellationToken);

        if (!string.IsNullOrWhiteSpace(existingImageUrl))
        {
            try
            {
                await DeleteAsync(existingImageUrl, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                // The old file is already absent; the replacement was saved successfully.
            }
        }

        return imageUrl;
    }

    public Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL is required.", nameof(imageUrl));

        if (!imageUrl.StartsWith(_rootUrl, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Image URL does not belong to the configured image storage.", nameof(imageUrl));

        var relativePath = imageUrl[_rootUrl.Length..]
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var rootFullPath = Path.GetFullPath(_rootPath);
        var resolvedPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (!resolvedPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Image URL resolves outside the configured image storage.");

        if (!File.Exists(resolvedPath))
            throw new FileNotFoundException("The image could not be found.", resolvedPath);

        try
        {
            File.Delete(resolvedPath);
        }
        catch (Exception exception)
        {
            throw new IOException("Failed to delete the image.", exception);
        }

        return Task.CompletedTask;
    }
}
