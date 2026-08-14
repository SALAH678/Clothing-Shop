using Application.Interfaces.Repositories;
using Domain.Categories;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class CategoryRepository(AppDbContext context) : Repository<Category>(context), ICategoryRepository
{
}
