namespace Application.Features.Dashboard.Dtos;

public sealed class OverviewDto
{
    public int TotalPurchases { get; init; }
    public int TotalProducts { get; init; }
    public int TotalUsers { get; init; }
    public int TotalCategories { get; init; }
}
