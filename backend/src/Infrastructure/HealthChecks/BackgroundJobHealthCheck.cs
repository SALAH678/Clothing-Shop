using Application.Common.Interfaces.BackgroundJobs;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace infrastructure.HealthChecks;

public class BackgroundJobHealthCheck(IBackgroundJobTracker tracker, string jobName,
    TimeSpan? maxAllowedStaleInterval = null) : IHealthCheck 
{
    private readonly IBackgroundJobTracker _tracker = tracker;
    private readonly string _jobName = jobName;
    private readonly TimeSpan? _maxAllowedStaleInterval = maxAllowedStaleInterval; // use this when background job scheduler periodically runs and
                                                                                   // you want to ensure it has run within a certain time frame 

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var lastExecution = _tracker.GetLastExecutionTime(_jobName);

        if (lastExecution is null)
        {
            return Task.FromResult(
                HealthCheckResult.Healthy($"Job '{_jobName}' is idle (no executions recorded since startup)."));
        }

        var timeSinceLastRun = DateTime.UtcNow - lastExecution.Value;

        // If a max threshold is defined and exceeded, report stale state
        if (_maxAllowedStaleInterval.HasValue && timeSinceLastRun > _maxAllowedStaleInterval.Value)
        {
            return Task.FromResult(
                HealthCheckResult.Degraded(
                    $"Job '{_jobName}' has been silent for {timeSinceLastRun.TotalMinutes:F1} mins."));
        }

        return Task.FromResult(
            HealthCheckResult.Healthy(
                $"Job '{_jobName}' executed successfully {timeSinceLastRun.TotalSeconds:F0}s ago."));
    }
}
