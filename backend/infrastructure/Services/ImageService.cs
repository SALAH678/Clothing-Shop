using Application.Common.Interfaces.Services;
using Domain.Common.Results;
using Domain.Products.Images;
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

    public async Task<Result<string>> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        if (content is null || content.Length == 0)
            return ImageErrors.InvalidFile;

        if (content.Length > MaxFileSizeBytes)
            return ImageErrors.FileTooLarge;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return ImageErrors.UnsupportedFormat;

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
        catch
        {
            return ImageErrors.SaveFailed;
        }

        return $"{_rootUrl}/{folder}/{uniqueFileName}";
    }

    public async Task<Result<string>> UpdateAsync(string? existingImageUrl, Stream newContent, string fileName, string folder, CancellationToken cancellationToken = default)
    {
        var saveResult = await SaveAsync(newContent, fileName, folder, cancellationToken);

        if (!saveResult.IsSuccess)
            return saveResult.TopError;

        if(!string.IsNullOrWhiteSpace(existingImageUrl))
            _ = await DeleteAsync(existingImageUrl, cancellationToken);
        // Best-effort cleanup of the old file — failure here doesn't fail the update.

        return saveResult.Value;
    }

    public Task<Result<Deleted>> DeleteAsync(string? imageUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return Task.FromResult<Result<Deleted>>(ImageErrors.NotFound);

        if (!imageUrl.StartsWith(_rootUrl, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<Result<Deleted>>(ImageErrors.NotFound);

        var relativePath = imageUrl[_rootUrl.Length..]
            .TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar);

        var rootFullPath = Path.GetFullPath(_rootPath);
        var resolvedPath = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (!resolvedPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult<Result<Deleted>>(ImageErrors.NotFound);

        if (!File.Exists(resolvedPath))
            return Task.FromResult<Result<Deleted>>(ImageErrors.NotFound);

        try
        {
            File.Delete(resolvedPath);
            return Task.FromResult<Result<Deleted>>(Result.Deleted);
        }
        catch
        {
            return Task.FromResult<Result<Deleted>>(ImageErrors.DeleteFailed);
        }
    }
}
