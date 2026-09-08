namespace Application.Common.Interfaces.BackgroundJobs;

public interface IImageCleanupJob
{
    Task ScheduleAsync(IReadOnlyCollection<string> imageUrls, CancellationToken cancellationToken);
}
