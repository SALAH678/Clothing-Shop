
using Application.Common.Interfaces;
using Domain.Carts;
using Domain.Users.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Carts.EventHandlers;

public sealed class UserCreatedDomainEventHandler(IUnitOfWork unitOfWork,ILogger<UserCreatedDomainEventHandler> logger)
    : INotificationHandler<UserCreatedDomainEvent>
{
    private readonly ILogger<UserCreatedDomainEventHandler> _logger = logger;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing automatic cart creation for UserId: {UserId}", notification.userId);

        var existingCart = await _unitOfWork.Carts.GetByUserIdAsync(notification.userId, cancellationToken);

        if (existingCart is not null)
        {
            _logger.LogWarning("Cart creation skipped: cart already exists for UserId: {UserId}", notification.userId);
            return;
        }

        var createCartResult = Cart.Create(notification.userId);

        if (createCartResult.IsError)
        {
            _logger.LogError("Failed to create domain cart object for UserId: {UserId}", notification.userId);
            return;
        }

        _logger.LogInformation("Creating cart for UserId: {UserId}", notification.userId);

        _unitOfWork.Carts.Create(createCartResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cart created successfully with CartId: {CartId} for UserId: {UserId}",
                createCartResult.Value.Id, notification.userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist cart changes to database for UserId: {UserId}", notification.userId);
            throw;
        }
    }
}
