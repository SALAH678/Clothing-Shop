using Domain.Carts;
using Domain.Carts.CartItems;
using Domain.Categories;
using Domain.Products;
using Domain.Products.Images;
using Domain.Products.Variants;
using Domain.Purchases;
using Domain.Purchases.Payments;
using Domain.Purchases.PurchaseItems;
using Domain.Users;
using Domain.Users.Accounts;
using Domain.Users.RefreshTokens;
using Domain.Users.VerificationTokens;
using Microsoft.EntityFrameworkCore;

namespace infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Variant> Variants => Set<Variant>();
    public DbSet<Image> Images => Set<Image>();

    public DbSet<User> Users => Set<User>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();

    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Payment> Payments => Set<Payment>();

    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}