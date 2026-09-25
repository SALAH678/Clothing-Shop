using Application.Common.Attributes;
using Application.Common.Interfaces;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviours;

public sealed class LoggingBehaviour<TRequest>(ILogger<TRequest> logger, IUser user) : IRequestPreProcessor<TRequest>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger = logger;
    private readonly IUser _user = user;

    public Task Process(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.UserId;
        var email = _user.Email ?? string.Empty;

        var requestProperties = typeof(TRequest).GetProperties()
            .ToDictionary(property => property.Name, property => Attribute.IsDefined(
                        property, typeof(SensitiveAttribute)) ? "[REDACTED]" : property.GetValue(request));

        _logger.LogInformation("Request: {Name} {@UserId} {@Email} {@Request}", requestName, userId, email, requestProperties);

        return Task.CompletedTask;
    }
}
