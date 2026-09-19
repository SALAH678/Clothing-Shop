using Application.Common.Interfaces.Repositories;
using Application.Features.Dashboard.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Dashboard.Query.GetOverview;

public class GetOverviewQueryHandler(IUserRepository userRepository, IProductRepository productRepository,
    IPurchaseRepository purchaseRepository, ICategoryRepository categoryRepository,
    ILogger<GetOverviewQueryHandler> logger) : IRequestHandler<GetOverviewQuery, OverviewDto>
{
    public async Task<OverviewDto> Handle(GetOverviewQuery request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetOverviewQuery");

        var userCount = await userRepository.GetTotalUsersNumber();
        var productCount = await productRepository.GetTotalProductsNumberAsync();
        var purchaseCount = await purchaseRepository.GetTotalPurchasesNumberAsync();
        var categoryCount = await categoryRepository.GetTotalCategoriesNumberAsync();

        var overview = new OverviewDto
        {
            TotalPurchases = purchaseCount,
            TotalProducts = productCount,
            TotalUsers = userCount,
            TotalCategories = categoryCount
        };

        return overview;
    }
}
