using Domain.Purchases.Payments;
using Domain.Purchases.Payments.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Sales;

public class PaymentConfiguration : AuditableEntityConfiguration<Payment>
{
    public override void Configure(EntityTypeBuilder<Payment> builder)
    {
        base.Configure(builder);

        builder.ToTable("Payments", schema: "sales", t =>
        {
            t.HasCheckConstraint("CK_Payments_Amount", "\"Amount\" >= 0");
            t.HasCheckConstraint("CK_Payments_Status", "\"Status\" IN ('Pending', 'Paid', 'Failed', 'Refunded')");
        });

        builder.Property(payment => payment.PurchaseId)
            .IsRequired();

        builder.Property(payment => payment.Amount)
            .IsRequired()
            .HasPrecision(20, 2);

        builder.Property(payment => payment.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(PaymentStatus.Pending);

        builder.Property(payment => payment.TransactionId)
            .IsRequired(false)
            .HasMaxLength(255)
            .HasDefaultValue(null);

        builder.HasIndex(payment => payment.TransactionId)
            .IsUnique();
    }
}