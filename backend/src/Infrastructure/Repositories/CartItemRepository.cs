using Application.Common.Interfaces.Repositories;
using Domain.Carts.CartItems;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class CartItemRepository(AppDbContext context) : Repository<CartItem>(context), ICartItemRepository
{
}
