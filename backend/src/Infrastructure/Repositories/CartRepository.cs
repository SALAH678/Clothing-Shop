using Application.Common.Interfaces.Repositories;
using Domain.Carts;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Repositories;

public sealed class CartRepository(AppDbContext context) : Repository<Cart>(context), ICartRepository
{
    public async Task<Cart?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _Context.Carts
            .Include(cart => cart.Items)
                .ThenInclude(item => item.Variant)
                    .ThenInclude(variant => variant.Product)
                        .ThenInclude(product => product.Images)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);

    public async Task<Cart?> GetByUserIdWithItemsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        await _Context.Carts
            .Include(cart => cart.Items)
            .FirstOrDefaultAsync(cart => cart.UserId == userId, cancellationToken);
}