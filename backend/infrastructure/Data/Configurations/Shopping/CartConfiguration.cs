using Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Shopping;

public class CartConfiguration : AuditableEntityConfiguration<Cart>
{
    public override void Configure(EntityTypeBuilder<Cart> builder)
    {
        base.Configure(builder);

        builder.ToTable("Carts", schema: "shopping");

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.HasMany(cart => cart.Items)
            .WithOne(cartItem => cartItem.Cart)
            .HasForeignKey(cartItem => cartItem.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(cart => cart.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}