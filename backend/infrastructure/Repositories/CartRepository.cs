using Application.Interfaces.Repositories;
using Domain.Carts;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class CartRepository(AppDbContext context) : Repository<Cart>(context), ICartRepository
{
}
