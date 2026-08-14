using Domain.Common;
using Domain.Common.Results;

namespace Domain.Products.Images;

public class Image : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public string ImageUrl { get; private set; } = null!;
    public bool IsMain { get; private set; }

    public Product Product { get; private set; } = null!;

    protected Image()
    {
    }

    protected Image(Guid productId, string imageUrl, bool isMain)
        : base(Guid.Empty)
    {
        ProductId = productId;
        ImageUrl = imageUrl;
        IsMain = isMain;
    }

    public static Result<Image> Create(Guid productId, string? imageUrl, bool isMain = false)
    {
        Error? error = Validate(productId, imageUrl);

        if (error is not null)
            return error.Value;

        return new Image(productId, imageUrl!.Trim(), isMain);
    }

    public Result<Updated> UpdateUrl(string? imageUrl)
    {
        Error? error = Validate(ProductId, imageUrl);

        if (error is not null)
            return error.Value;

        ImageUrl = imageUrl!.Trim();

        return Result.Updated;
    }

    public Result<Updated> MarkAsMain()
    {
        IsMain = true;

        return Result.Updated;
    }

    public Result<Updated> UnmarkAsMain()
    {
        IsMain = false;

        return Result.Updated;
    }

    private static Error? Validate(Guid productId, string? imageUrl)
    {
        if (productId == Guid.Empty)
            return ImageErrors.ProductIdRequired;

        if (string.IsNullOrWhiteSpace(imageUrl))
            return ImageErrors.ImageUrlRequired;

        if (imageUrl.Length > 500)
            return ImageErrors.ImageUrlTooLong;

        if (!IsValidUrl(imageUrl))
            return ImageErrors.InvalidImageUrl;

        return null;
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) &&
               (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
