using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Carts.Dtos;
using AutoMapper;
using Domain.Carts;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Carts.Commands.CreateCart;

public class CreateCartCommandHandler(IUnitOfWork unitOfWork,
    IMapper mapper, IUser user) : IRequestHandler<CreateCartCommand, Result<CartDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly IUser _user = user;

    public async Task<Result<CartDto>> Handle(CreateCartCommand request, CancellationToken cancellationToken)
    {
        var existingCart = await _unitOfWork.Carts.GetByUserIdAsync(_user.UserId, cancellationToken);

        if (existingCart is not null)
            return ApplicationErrors.CartAlreadyExists;

        var createCartResult = Cart.Create(_user.UserId);

        if (createCartResult.IsError)
            return createCartResult.Errors;

        _unitOfWork.Carts.Create(createCartResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            return ApplicationErrors.CartOperationFailed;
        }

        return _mapper.Map<CartDto>(createCartResult.Value);
    }
}