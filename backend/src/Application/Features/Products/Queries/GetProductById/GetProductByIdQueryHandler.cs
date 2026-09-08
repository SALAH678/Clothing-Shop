using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Products.Queries.GetProductById;

public class GetProductByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductByIdQueryHandler> logger, IUser user)
    : IRequestHandler<GetProductByIdQuery, Result<ProductDto>>
{
    private readonly ILogger<GetProductByIdQueryHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProductDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get product requested for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
            request.ProductId, _user.Email, _user.UserId);

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            _logger.LogWarning("Get product failed: product not found for ProductId: {ProductId}, Email: {Email}, UserId: {UserId}",
                request.ProductId, _user.Email, _user.UserId);
            return Error.NotFound(
                code: "Product_Not_Found",
                description: $"Product with ID {request.ProductId} was not found.");
        }

        return _mapper.Map<ProductDto>(product);
    }
}
