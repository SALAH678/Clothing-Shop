using Domain.Carts.CartItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations;

public class CartItemConfiguration : AuditableEntityConfiguration<CartItem>
{
    public override void Configure(EntityTypeBuilder<CartItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("CartItems", schema: "shopping", t =>
        {
            t.HasCheckConstraint(
                name: "CK_CartItems_Quantity",
                sql: "\"Quantity\" >= 0");
        });

        builder.Property(c => c.CartId)
            .IsRequired();

        builder.Property(c => c.VariantId)
            .IsRequired();

        builder.Property(cartItem => cartItem.Quantity)
            .IsRequired();

        builder.HasIndex(cartItem => new { cartItem.CartId, cartItem.VariantId })
            .IsUnique();
    }
}