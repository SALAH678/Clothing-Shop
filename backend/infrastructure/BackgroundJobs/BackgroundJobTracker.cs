using Application.Common.Interfaces.BackgroundJobs;
using System.Collections.Concurrent;

namespace infrastructure.BackgroundJobs;

public class BackgroundJobTracker : IBackgroundJobTracker
{
    private readonly ConcurrentDictionary<string, DateTime> _lastExecutions = new();

    public void RecordHeartbeat(string jobName)
    {
        _lastExecutions[jobName] = DateTime.UtcNow;
    }

    public DateTime? GetLastExecutionTime(string jobName)
    {
        return _lastExecutions.TryGetValue(jobName, out var lastTime) ? lastTime : null;
    }
}
