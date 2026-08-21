
namespace Application.Common.Interfaces.BackgroundJobs;

public interface IEmailJob
{
    public Task ScheduleSendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    public Task ScheduleSendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default);
}
