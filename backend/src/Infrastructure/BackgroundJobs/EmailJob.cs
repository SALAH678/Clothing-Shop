using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Repositories;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace infrastructure.BackgroundJobs;

public record SendCodePayload(string Email, string Code);

//public record PurchaseNotificationItemPayload(
//    string ProductName,
//    string Size,
//    string Color,
//    decimal Price,
//    int Quantity);

public record SendPurchaseNotificationPayload(
    Guid PurchaseId,
    string FullName,
    string PhoneNumber,
    List<PurchaseNotificationItemPayload> Items,
    decimal TotalAmount);

public class EmailJob(IEmailService emailService, ITimeTickerManager<TimeTickerEntity> tickerManager,
    ILogger<EmailJob> logger, IBackgroundJobTracker backgroundJobTracker, IUserRepository userRepository) : IEmailJob
{
    private readonly IEmailService _emailService = emailService;
    private readonly ITimeTickerManager<TimeTickerEntity> _tickerManager = tickerManager;
    private readonly ILogger<EmailJob> _logger = logger;
    private readonly IBackgroundJobTracker _backgroundJobTracker = backgroundJobTracker;
    private readonly IUserRepository _userRepository = userRepository;

    [TickerFunction(functionName: "SendVerificationCode")]
    public async Task SendVerificationCodeAsync(TickerFunctionContext<SendCodePayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;
        _logger.LogInformation("Starting background job to send verification code to {Email}", payload.Email);

        try
        {
            await _emailService.SendVerificationCodeAsync(payload.Email, payload.Code, cancellationToken);
            _logger.LogInformation("Verification code successfully sent to {Email}", payload.Email);

            _backgroundJobTracker.RecordHeartbeat(nameof(EmailJob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send verification code to {Email} via background job", payload.Email);
        }
    }

    [TickerFunction(functionName: "SendPasswordResetCode")]
    public async Task SendPasswordResetCodeAsync(TickerFunctionContext<SendCodePayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;
        _logger.LogInformation("Starting background job to send password reset code to {Email}", payload.Email);

        try
        {
            await _emailService.SendPasswordResetCodeAsync(payload.Email, payload.Code, cancellationToken);
            _logger.LogInformation("Password reset code successfully sent to {Email}", payload.Email);

            _backgroundJobTracker.RecordHeartbeat(nameof(EmailJob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset code to {Email} via background job", payload.Email);
        }
    }

    [TickerFunction(functionName: "SendPurchaseNotification")]
    public async Task SendPurchaseNotificationAsync(TickerFunctionContext<SendPurchaseNotificationPayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;
        _logger.LogInformation("Starting background job to send purchase notification for purchase {PurchaseId}", payload.PurchaseId);

        try
        {
            await _emailService.SendPurchaseNotificationAsync(payload.PurchaseId, payload.FullName, payload.PhoneNumber, payload.Items, payload.TotalAmount, cancellationToken);
            _logger.LogInformation("Purchase notification successfully sent for purchase {PurchaseId}", payload.PurchaseId);

            _backgroundJobTracker.RecordHeartbeat(nameof(EmailJob));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Failed to send purchase notification for purchase {PurchaseId} via background job", payload.PurchaseId);
        }
    }

    public async Task ScheduleSendPurchaseNotificationAsync(Guid purchaseId, Guid userId, List<PurchaseNotificationItemPayload> items, decimal totalAmount,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Scheduling purchase notification email job for purchase {PurchaseId}", purchaseId);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            _logger.LogError("User not found for ID {UserId}", userId);
            return;
        }

        var fullName = user.LastName + " " + user.FirstName;
        var phoneNumber = user.PhoneNumber.Value;

        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "SendPurchaseNotification",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(
                new SendPurchaseNotificationPayload(purchaseId, fullName, phoneNumber, items, totalAmount))
        }, cancellationToken);
    }

    public async Task ScheduleSendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Scheduling password reset code email job for {Email}", email);
        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "SendPasswordResetCode",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(new SendCodePayload(email, code))
        }, cancellationToken);
    }

    public async Task ScheduleSendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Scheduling verification code email job for {Email}", email);
        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "SendVerificationCode",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(new SendCodePayload(email, code))
        }, cancellationToken);
    }
}
