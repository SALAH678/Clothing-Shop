using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace infrastructure.BackgroundJobs;

public class ImageCleanupJob(IImageService imageService, ITimeTickerManager<TimeTickerEntity> tickerManager) : IImageCleanupJob
{
    private readonly IImageService _imageService = imageService;
    private readonly ITimeTickerManager<TimeTickerEntity> _tickerManager = tickerManager;

    [TickerFunction(functionName: "DeleteImages")]
    public async Task DeleteImagesAsync(TickerFunctionContext<List<string>> tickerContext, CancellationToken cancellationToken)
    {
        var imageUrls = tickerContext.Request;

        if (imageUrls is null || imageUrls.Count == 0)
            return;

        foreach (var imageUrl in imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)))
        {
            try
            {
                await _imageService.DeleteAsync(imageUrl, cancellationToken);
            }
            catch (FileNotFoundException)
            {
                // The image is already absent, so cleanup is complete.
            }
        }
    }

    public async Task ScheduleAsync(IReadOnlyCollection<string> imageUrls, CancellationToken cancellationToken)
    {
        if (imageUrls is null || imageUrls.Count == 0)
            return;

        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "DeleteImages",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(imageUrls)
        }, cancellationToken);
    }
}