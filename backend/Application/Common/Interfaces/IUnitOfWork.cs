using Application.Common.Interfaces.Repositories;

namespace Application.Common.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IAccountRepository Accounts { get; }
    ICartItemRepository CartItems { get; }
    ICartRepository Carts { get; }
    ICategoryRepository Categories { get; }
    IImageRepository Images { get; }
    IPaymentRepository Payments { get; }
    IProductRepository Products { get; }
    IPurchaseItemRepository PurchaseItems { get; }
    IPurchaseRepository Purchases { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IUserRepository Users { get; }
    IVariantRepository Variants { get; }
    IVerificationTokenRepository VerificationTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}