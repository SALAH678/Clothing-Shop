using Domain.Products.Variants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Catalog;

public class VariantConfiguration : AuditableEntityConfiguration<Variant>
{
    public override void Configure(EntityTypeBuilder<Variant> builder)
    {
        base.Configure(builder);

        builder.ToTable("Variants", schema: "catalog", t =>
        {
            t.HasCheckConstraint("CK_Variant_StockQuantity_NonNegative", "\"StockQuantity\" >= 0");
        });

        builder.Property(variant => variant.ProductId)
            .IsRequired();

        builder.Property(variant => variant.Size)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(variant => variant.Color)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(variant => variant.StockQuantity)
            .IsRequired();

        builder.HasMany(variant => variant.CartItems)
            .WithOne(cartItem => cartItem.Variant)
            .HasForeignKey(cartItem => cartItem.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(variant => variant.PurchaseItems)
            .WithOne(purchaseItem => purchaseItem.Variant)
            .HasForeignKey(purchaseItem => purchaseItem.VariantId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Navigation(variant => variant.PurchaseItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(variant => variant.CartItems)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}