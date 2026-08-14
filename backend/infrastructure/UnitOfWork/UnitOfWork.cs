using Application.Common.Interfaces;
using Application.Interfaces.Repositories;
using infrastructure.Data;

namespace infrastructure.UnitOfWork;

public sealed class UnitOfWork(
    AppDbContext context, IAccountRepository accounts, ICartItemRepository cartItems, ICartRepository carts, ICategoryRepository categories, IImageRepository images,
    IPaymentRepository payments, IProductRepository products, IPurchaseItemRepository purchaseItems, IPurchaseRepository purchases, IRefreshTokenRepository refreshTokens,
    IUserRepository users, IVariantRepository variants, IVerificationTokenRepository verificationTokens) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    public IAccountRepository Accounts { get; } = accounts;
    public ICartItemRepository CartItems { get; } = cartItems;
    public ICartRepository Carts { get; } = carts;
    public ICategoryRepository Categories { get; } = categories;
    public IImageRepository Images { get; } = images;
    public IPaymentRepository Payments { get; } = payments;
    public IProductRepository Products { get; } = products;
    public IPurchaseItemRepository PurchaseItems { get; } = purchaseItems;
    public IPurchaseRepository Purchases { get; } = purchases;
    public IRefreshTokenRepository RefreshTokens { get; } = refreshTokens;
    public IUserRepository Users { get; } = users;
    public IVariantRepository Variants { get; } = variants;
    public IVerificationTokenRepository VerificationTokens { get; } = verificationTokens;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public void Dispose() => _context.Dispose();
}