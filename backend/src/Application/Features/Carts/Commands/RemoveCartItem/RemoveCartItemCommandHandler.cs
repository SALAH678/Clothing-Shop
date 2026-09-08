using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user, ILogger<RemoveCartItemCommandHandler> logger) : IRequestHandler<RemoveCartItemCommand, Result<Deleted>>
{
    private readonly ILogger<RemoveCartItemCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<Deleted>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Remove cart item requested for CartItemId: {CartItemId}, Email: {Email}, UserId: {UserId}",
            request.CartItemId, _user.Email, _user.UserId);

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
        {
            _logger.LogWarning("Remove cart item failed: cart not found for CartItemId: {CartItemId}, Email: {Email}, UserId: {UserId}",
                request.CartItemId, _user.Email, _user.UserId);
            return ApplicationErrors.CartNotExist;
        }

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);

        if (cartItem is null)
        {
            _logger.LogWarning("Remove cart item failed: cart item not found for CartItemId: {CartItemId}, Email: {Email}, UserId: {UserId}",
                request.CartItemId, _user.Email, _user.UserId);
            return CartErrors.ItemNotFound;
        }

        _unitOfWork.CartItems.Delete(cartItem);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Remove cart item failed: unable to save changes for CartItemId: {CartItemId}, Email: {Email}, UserId: {UserId}",
                request.CartItemId, _user.Email, _user.UserId);
            return ApplicationErrors.CartOperationFailed;
        }

        _logger.LogInformation("Cart item removed successfully for CartItemId: {CartItemId}, Email: {Email}, UserId: {UserId}",
            request.CartItemId, _user.Email, _user.UserId);

        return Result.Deleted;
    }
}