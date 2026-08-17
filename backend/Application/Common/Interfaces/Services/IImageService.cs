using Domain.Common.Results;

namespace Application.Common.Interfaces.Services;

public interface IImageService
{
    Task<Result<string>> SaveAsync(Stream content, string fileName, string folder, CancellationToken cancellationToken = default);

    Task<Result<string>> UpdateAsync(string? existingImageUrl, Stream newContent, string fileName, string folder, CancellationToken cancellationToken = default);

    Task<Result<Deleted>> DeleteAsync(string? imageUrl, CancellationToken cancellationToken = default);
}
