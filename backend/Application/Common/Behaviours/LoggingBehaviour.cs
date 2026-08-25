using Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user)
    : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger _logger = logger;
    private readonly IUser _user = user;

    public async Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user?.UserId == null ? Guid.Empty : _user.UserId;
        string? email = _user?.Email ?? string.Empty;

        _logger.LogInformation(
            "Request: {Name} {@UserId} {@Email} {@Request}", requestName, userId, email, request);
    }
}
