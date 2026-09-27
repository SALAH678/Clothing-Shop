using Domain.Products.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Catalog;

public class ImageConfiguration : AuditableEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        base.Configure(builder);

        builder.ToTable("Images", schema: "catalog");

        builder.Property(image => image.ProductId)
            .IsRequired();

        builder.Property(image => image.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(image => image.IsMain)
            .IsRequired();
    }
}