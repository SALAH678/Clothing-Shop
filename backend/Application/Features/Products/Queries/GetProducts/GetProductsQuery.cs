using Application.Common.Models;
using Application.Features.Products.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Queries.GetProducts;

public sealed record ProductFilter(
    string? Search,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? size,
    string? color,
    string? SortBy,
    bool Descending);

public sealed record GetProductsQuery(
    Guid CategoryId,
    int PageNumber,
    int PageSize,
    ProductFilter Filter) : IRequest<Result<PaginatedList<ProductDto>>>;
