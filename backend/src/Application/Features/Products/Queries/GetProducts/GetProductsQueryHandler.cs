using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductsQueryHandler> logger, IUser user) : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly ILogger<GetProductsQueryHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get products requested for CategoryId: {CategoryId}, Filter: {@Filter}, PageNumber: {PageNumber}, PageSize: {PageSize}, Email: {Email}, UserId: {UserId}",
            request.CategoryId, request.Filter, request.PageNumber, request.PageSize, _user.Email, _user.UserId);

        var products = await _unitOfWork.Products.GetProductsAsync(request.CategoryId, request.Filter, request.PageNumber, request.PageSize, cancellationToken);

        var mapped = _mapper.Map<PaginatedList<ProductDto>>(products);

        _logger.LogInformation("Get products succeeded with Count: {Count}, Email: {Email}, UserId: {UserId}",
            mapped.Items?.Count ?? 0, _user.Email, _user.UserId);

        return mapped;
    }
}
