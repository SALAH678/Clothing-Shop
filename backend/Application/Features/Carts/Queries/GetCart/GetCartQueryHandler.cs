using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Queries.GetCart;

public class GetCartQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (cart is null)
            return CartErrors.CartRequired;

        return _mapper.Map<CartDto>(cart);
    }
}