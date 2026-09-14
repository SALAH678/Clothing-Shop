
namespace Application.Common.Interfaces.BackgroundJobs;

public interface IEmailJob
{
    public Task ScheduleSendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    public Task ScheduleSendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task ScheduleSendPurchaseNotificationAsync(Guid purchaseId, Guid userId, List<PurchaseNotificationItemPayload> items, decimal totalAmount,
        CancellationToken cancellationToken = default);
}

public class PurchaseNotificationItemPayload
{
    public string ProductName { get; set; } = default!;
    public string Size { get; set; } = default!;
    public string Color { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}