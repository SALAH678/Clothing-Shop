using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Carts.Commands.AddCartItem;

public class AddCartItemCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user, ILogger<AddCartItemCommandHandler> logger) : IRequestHandler<AddCartItemCommand, Result<CartDto>>
{
    private readonly ILogger<AddCartItemCommandHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<CartDto>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Add cart item requested for VariantId: {VariantId}, Quantity: {Quantity}, Email: {Email}, UserId: {UserId}",
            request.VariantId, request.Quantity, _user.Email, _user.UserId);

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
        {
            _logger.LogWarning("Add cart item failed: cart not found for VariantId: {VariantId}, Email: {Email}, UserId: {UserId}",
                request.VariantId, _user.Email, _user.UserId);
            return ApplicationErrors.CartNotExist;
        }

        var addItemResult = cart.AddItem(request.VariantId, request.Quantity);

        if (addItemResult.IsError)
        {
            _logger.LogWarning("Add cart item failed for VariantId: {VariantId}, Email: {Email}, UserId: {UserId}",
                request.VariantId, _user.Email, _user.UserId);
            return addItemResult.Errors;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cart item added successfully for VariantId: {VariantId}, CartId: {CartId}, Email: {Email}, UserId: {UserId}",
            request.VariantId, cart.Id, _user.Email, _user.UserId);

        var item = cart.Items.First(i => i.VariantId == request.VariantId);

        if (item.Variant is null)
        {
            var variant = await _unitOfWork.Variants.GetByIdAsync(request.VariantId, cancellationToken);
            item.AttachVariant(variant!);
        }
        // u need to review the object returned right now is overload  
        return _mapper.Map<CartDto>(cart);
    }
}