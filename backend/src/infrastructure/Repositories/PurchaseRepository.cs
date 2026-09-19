using Application.Common.Interfaces.Repositories;
using Application.Common.Models;
using Application.Features.Purchases.Dtos;
using Domain.Purchases;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class PurchaseRepository(AppDbContext context) : Repository<Purchase>(context), IPurchaseRepository
{
    public async Task<PaginatedList<PurchaseDto>> GetAllPurchasesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _Context.Purchases.AsNoTracking()
            .Select(p => new PurchaseDto
            {
                PurchaseId = p.Id,
                CustomerName = p.User.FirstName + " " + p.User.LastName,
                CustomerPhoneNumber = p.CustomerPhone.ToString(),
                Wilaya = p.CustomerAddress.Wilaya,
                City = p.CustomerAddress.City,
                Street = p.CustomerAddress.Street,
                PurchaseItems = p.Items.Select(i => new PurchaseItemDto
                {
                    Quantity = i.Quantity,
                    ProductName = i.Variant.Product.Name,
                    UnitPrice = i.UnitPrice
                }).ToList(),
                TotalAmount = p.TotalAmount,
                Status = p.Payment.Status.ToString(),
                Date = p.CreatedAtUtc
            });
            

        var itemsNumber = await query.CountAsync(cancellationToken);

        var purchases = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        PaginatedList<PurchaseDto> result = new PaginatedList<PurchaseDto>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = itemsNumber,
            TotalPages = (int)Math.Ceiling((double)itemsNumber / pageSize),//Math.Ceiling(3.1); the result is 4
            Items = purchases
        };

        return result;
    }   

    public async Task<int> GetTotalPurchasesNumberAsync(CancellationToken cancellationToken = default) =>
        await _Context.Purchases.CountAsync(cancellationToken);
}
