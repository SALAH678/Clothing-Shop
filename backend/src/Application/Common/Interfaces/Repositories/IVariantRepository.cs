using Domain.Products.Variants;

namespace Application.Common.Interfaces.Repositories;

public interface IVariantRepository : IRepository<Variant>
{
    Task<List<Variant>> GetByIdsWithProductAsync(List<Guid> ids, CancellationToken cancellationToken = default);
}
