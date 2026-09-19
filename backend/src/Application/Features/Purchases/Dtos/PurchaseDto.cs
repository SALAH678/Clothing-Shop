namespace Application.Features.Purchases.Dtos;

public sealed class PurchaseDto
{
    public Guid PurchaseId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string? CustomerPhoneNumber { get; init; }
    public string Wilaya { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public List<PurchaseItemDto> PurchaseItems { get; init; } = [];
    public decimal TotalAmount { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset Date { get; init; }
}

public sealed class PurchaseItemDto
{
    public int Quantity { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public decimal UnitPrice { get; init; }
}