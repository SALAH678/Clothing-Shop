namespace Application.Common.Interfaces.BackgroundJobs;
public interface IBackgroundJobTracker
{
    void RecordHeartbeat(string jobName);
    DateTime? GetLastExecutionTime(string jobName);
}
