using Application.Common.Models;
using Application.Features.Purchases.Dtos;
using MediatR;

namespace Application.Features.Purchases.Queries.GetPurchases;

public sealed record GetPurchasesQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<PaginatedList<PurchaseDto>>;




