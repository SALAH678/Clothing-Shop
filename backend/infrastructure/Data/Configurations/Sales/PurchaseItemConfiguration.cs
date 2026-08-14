using Domain.Purchases.PurchaseItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Sales;

public class PurchaseItemConfiguration : AuditableEntityConfiguration<PurchaseItem>
{
    public override void Configure(EntityTypeBuilder<PurchaseItem> builder)
    {
        base.Configure(builder);

        builder.ToTable("PurchaseItems", schema: "sales", t =>
        {
            t.HasCheckConstraint("CK_PurchaseItems_Quantity", "\"Quantity\" > 0");
        });

        builder.Property(purchaseItem => purchaseItem.PurchaseId)
            .IsRequired();

        builder.Property(purchaseItem => purchaseItem.VariantId)
            .IsRequired();

        builder.Property(purchaseItem => purchaseItem.Quantity)
            .IsRequired();

        builder.Property(purchaseItem => purchaseItem.UnitPrice)
            .IsRequired()
            .HasPrecision(20, 2);
    }
}