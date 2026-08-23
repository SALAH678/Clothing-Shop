using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace infrastructure.BackgroundJobs;

public class ImageCleanupJob(IImageService imageService, ITimeTickerManager<TimeTickerEntity> tickerManager, ILogger<ImageCleanupJob> logger) : IImageCleanupJob
{
    private readonly IImageService _imageService = imageService;
    private readonly ITimeTickerManager<TimeTickerEntity> _tickerManager = tickerManager;
    private readonly ILogger<ImageCleanupJob> _logger = logger;

    [TickerFunction(functionName: "DeleteImages")]
    public async Task DeleteImagesAsync(TickerFunctionContext<List<string>> tickerContext, CancellationToken cancellationToken)
    {
        var imageUrls = tickerContext.Request;

        if (imageUrls is null || imageUrls.Count == 0)
            return;

        _logger.LogInformation("Starting background job to delete {Count} images", imageUrls.Count);

        foreach (var imageUrl in imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
        {
            try
            {
                await _imageService.DeleteAsync(imageUrl, cancellationToken);
                _logger.LogInformation("Successfully deleted image: {ImageUrl}", imageUrl);
            }
            catch (FileNotFoundException)
            {
                _logger.LogWarning("Image cleanup: Image was already absent at URL: {ImageUrl}", imageUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete image at URL: {ImageUrl}", imageUrl);
            }
        }
    }

    public async Task ScheduleAsync(IReadOnlyCollection<string> imageUrls, CancellationToken cancellationToken)
    {
        if (imageUrls is null || imageUrls.Count == 0)
            return;

        _logger.LogInformation("Scheduling cleanup background job for {Count} images", imageUrls.Count);

        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "DeleteImages",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(imageUrls)
        }, cancellationToken);
    }
}