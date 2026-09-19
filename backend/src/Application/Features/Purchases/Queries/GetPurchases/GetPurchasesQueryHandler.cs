using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.Purchases.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Purchases.Queries.GetPurchases;

public class GetPurchasesQueryHandler(IPurchaseRepository purchaseRepository,
    ILogger<GetPurchasesQueryHandler> logger) : IRequestHandler<GetPurchasesQuery, PaginatedList<PurchaseDto>>
{
    public async Task<PaginatedList<PurchaseDto>> Handle(GetPurchasesQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetPurchasesQuery with PageNumber: {PageNumber}, PageSize: {PageSize}", request.PageNumber, request.PageSize);

        PaginatedList<PurchaseDto> purchases = await purchaseRepository.GetAllPurchasesAsync(request.PageNumber, request.PageSize, cancellationToken);

        return purchases;
    }
}
