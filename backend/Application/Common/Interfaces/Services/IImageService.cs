namespace Application.Common.Interfaces.Services;

public interface IImageService
{
    Task<string> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default);

    Task<string> UpdateAsync(string? existingImageUrl, Stream newContent, string fileName, string folder, CancellationToken cancellationToken = default);

    Task DeleteAsync(string imageUrl, CancellationToken cancellationToken = default);
}
