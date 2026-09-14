
namespace Application.Common.Interfaces.BackgroundJobs;

public interface IEmailJob
{
    public Task ScheduleSendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    public Task ScheduleSendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task ScheduleSendPurchaseNotificationAsync(Guid purchaseId, Guid userId, List<PurchaseNotificationItemPayload> items, decimal totalAmount,
        CancellationToken cancellationToken = default);
}

public record PurchaseNotificationItemPayload(
    string ProductName,
    string Size,
    string Color,
    decimal Price,
    int Quantity);