using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public class GetProductsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetProductsQuery, Result<PaginatedList<ProductDto>>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<PaginatedList<ProductDto>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _unitOfWork.Products.GetProductsAsync(request.CategoryId, request.Filter, cancellationToken);

        return _mapper.Map<PaginatedList<ProductDto>>(products);
    }
}
