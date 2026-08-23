using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace infrastructure.BackgroundJobs;

public record SendCodePayload(string Email, string Code);

public class EmailJob(IEmailService emailService, ITimeTickerManager<TimeTickerEntity> tickerManager, ILogger<EmailJob> logger) : IEmailJob
{
    private readonly IEmailService _emailService = emailService;
    private readonly ITimeTickerManager<TimeTickerEntity> _tickerManager = tickerManager;
    private readonly ILogger<EmailJob> _logger = logger;

    [TickerFunction(functionName: "SendVerificationCode")]
    public async Task SendVerificationCodeAsync(TickerFunctionContext<SendCodePayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;
        _logger.LogInformation("Starting background job to send verification code to {Email}", payload.Email);

        try
        {
            await _emailService.SendVerificationCodeAsync(payload.Email, payload.Code, cancellationToken);
            _logger.LogInformation("Verification code successfully sent to {Email}", payload.Email);
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send password reset code to {Email} via background job", payload.Email);
        }
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
