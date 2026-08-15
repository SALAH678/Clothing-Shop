namespace Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
}
