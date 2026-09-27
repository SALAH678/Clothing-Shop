using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Purchases.Enum;
using Domain.Purchases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Sales;

public class PurchaseConfiguration : AuditableEntityConfiguration<Purchase>
{
    public override void Configure(EntityTypeBuilder<Purchase> builder)
    {
        base.Configure(builder);

        builder.ToTable("Purchases", schema: "sales");

        builder.Property(purchase => purchase.UserId)
            .IsRequired();

        builder.Property(purchase => purchase.TotalAmount)
            .IsRequired()
            .HasPrecision(20, 2);

        builder.Property(purchase => purchase.CustomerPhone)
            .IsRequired()
            .HasConversion(
               CustomerPhone => CustomerPhone.Value,
               value => PhoneNumber.Create(value).Value)
            .HasMaxLength(15);

        builder.Property(Purchase => Purchase.Origin)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.OwnsOne(purchase => purchase.CustomerAddress, address =>
        {
            address.Property(addressValue => addressValue.Street).HasColumnName("Street").IsRequired().HasMaxLength(200).HasColumnName("Street");
            address.Property(addressValue => addressValue.City).HasColumnName("City").IsRequired().HasMaxLength(100).HasColumnName("City");
            address.Property(addressValue => addressValue.Wilaya).HasColumnName("Wilaya").IsRequired().HasMaxLength(100).HasColumnName("Wilaya");
        });

        builder.HasMany(purchase => purchase.Items)
            .WithOne(item => item.Purchase)
            .HasForeignKey(item => item.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(purchase => purchase.Payment)
            .WithOne(payment => payment.Purchase)
            .HasForeignKey<Domain.Purchases.Payments.Payment>(payment => payment.PurchaseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(purchase => purchase.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}