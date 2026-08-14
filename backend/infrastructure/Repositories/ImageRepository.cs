using Application.Common.Interfaces.Repositories;
using Domain.Products.Images;
using infrastructure.Data;
using infrastructure.Repositories.Abstractions;

namespace infrastructure.Repositories;

public sealed class ImageRepository(AppDbContext context) : Repository<Image>(context), IImageRepository
{
}
