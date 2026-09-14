using Application.Common.Interfaces.BackgroundJobs;

namespace Application.Common.Interfaces.Services;

public interface IEmailService
{
    Task SendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task SendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task SendPurchaseNotificationAsync(Guid purchaseId, string fullName, string phoneNumber, List<PurchaseNotificationItemPayload> items, decimal totalAmount,
        CancellationToken cancellationToken = default);
}
