using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.AddCartItem;

public class AddCartItemCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user) : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
            return ApplicationErrors.CartNotExist;

        var addItemResult = cart.AddItem(request.VariantId, request.Quantity);

        if (addItemResult.IsError)
            return addItemResult.Errors;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.CartOperationFailed;
        }

        return _mapper.Map<CartDto>(cart);
    }
}