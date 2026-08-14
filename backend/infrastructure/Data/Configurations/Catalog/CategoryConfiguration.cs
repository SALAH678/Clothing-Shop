using Domain.Categories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Catalog;

public class CategoryConfiguration : AuditableEntityConfiguration<Category>
{
    public override void Configure(EntityTypeBuilder<Category> builder)
    {
        base.Configure(builder);

        builder.ToTable("Categories", schema: "catalog");

        builder.Property(category => category.CategoryName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(category => category.ImageUrl)
            .IsRequired(false)
            .HasMaxLength(500)
            .HasDefaultValue(null);

        builder.Property(category => category.ParentCategoryId)
            .IsRequired(false);

        builder.HasOne(category => category.ParentCategory)
            .WithMany(parent => parent.Subcategories)
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(category => category.Subcategories)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(category => category.Products)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}