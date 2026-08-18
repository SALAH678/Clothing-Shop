using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.RemoveCartItem;

public class RemoveCartItemCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user) : IRequestHandler<RemoveCartItemCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<Deleted>> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
            return CartErrors.CartRequired;

        var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);

        if(cartItem is null)
            return CartErrors.ItemNotFound;

        _unitOfWork.CartItems.Delete(cartItem);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.CartOperationFailed;
        }

        return Result.Deleted;
    }
}