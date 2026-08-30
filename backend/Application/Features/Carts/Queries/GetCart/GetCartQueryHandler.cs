using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Carts.Queries.GetCart;

public class GetCartQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user, ILogger<GetCartQueryHandler> logger) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    private readonly ILogger<GetCartQueryHandler> _logger = logger;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get cart requested for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
        {
            _logger.LogWarning("Get cart failed: cart not found for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);
            return CartErrors.CartRequired;
        }

        return _mapper.Map<CartDto>(cart);
    }
}