using Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Catalog;

public class ProductConfiguration : AuditableEntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> builder)
    {
        base.Configure(builder);

        builder.ToTable("Products", schema: "catalog");

        builder.Property(product => product.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(product => product.Description)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(product => product.BasePrice)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(product => product.Discount)
            .IsRequired(false)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.Property(product => product.CategoryId)
            .IsRequired();

        builder.HasOne(product => product.Category)
            .WithMany(category => category.Products)
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(product => product.Variants)
            .WithOne(variant => variant.Product)
            .HasForeignKey(variant => variant.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(product => product.Images)
            .WithOne(image => image.Product)
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(product => product.Name);

        builder.Navigation(product => product.Variants)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(product => product.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}