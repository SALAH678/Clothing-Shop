using Application.Common.Interfaces.BackgroundJobs;
using Application.Common.Interfaces.Services;
using TickerQ.Utilities;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace infrastructure.BackgroundJobs;

public record SendCodePayload(string Email, string Code);

public class EmailJob(IEmailService emailService, ITimeTickerManager<TimeTickerEntity> tickerManager) : IEmailJob
{
    private readonly IEmailService _emailService = emailService;
    private readonly ITimeTickerManager<TimeTickerEntity> _tickerManager = tickerManager;

    [TickerFunction(functionName: "SendVerificationCode")]
    public async Task SendVerificationCodeAsync(TickerFunctionContext<SendCodePayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;

        try
        {
            await _emailService.SendVerificationCodeAsync(payload.Email, payload.Code, cancellationToken);
        }
        catch
        {
            //logging can be added here if needed
        }
    }

    [TickerFunction(functionName: "SendPasswordResetCode")]
    public async Task SendPasswordResetCodeAsync(TickerFunctionContext<SendCodePayload> tickerContext, CancellationToken cancellationToken = default)
    {
        var payload = tickerContext.Request;

        try
        {
            await _emailService.SendPasswordResetCodeAsync(payload.Email, payload.Code, cancellationToken);
        }
        catch
        {
            //logging can be added here if needed
        }
    }

    public async Task ScheduleSendPasswordResetCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "SendPasswordResetCode",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(new SendCodePayload(email, code))
        }, cancellationToken);
    }

    public async Task ScheduleSendVerificationCodeAsync(string email, string code, CancellationToken cancellationToken = default)
    {
        await _tickerManager.AddAsync(new TimeTickerEntity
        {
            Function = "SendVerificationCode",
            ExecutionTime = DateTime.UtcNow,
            Request = TickerHelper.CreateTickerRequest(new SendCodePayload(email, code))
        }, cancellationToken);
    }
}
