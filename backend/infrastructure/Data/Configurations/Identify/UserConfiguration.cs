using Domain.Carts;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users;
using Domain.Users.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Identify;

public class UserConfiguration : AuditableEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("Users", schema: "identity", t =>
        {
            t.HasCheckConstraint(
                name: "CK_USER_ROLE",
                sql: "\"Role\" IN ('Admin', 'Customer')"
                );
        });

        builder.Property(user => user.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(user => user.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(user => user.EmailVerified)
            .IsRequired();

        builder.Property(user => user.UserRole)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(Role.Customer);

        builder.Property(user => user.Email)
            .IsRequired()
            .HasConversion(
               email => email.Value,
               value => Email.Create(value).Value)
            .HasMaxLength(200);

        builder.HasIndex(user => user.Email.Value)
            .IsUnique()
            .HasDatabaseName("IX_User_Email");

        builder.Property(user => user.PhoneNumber)
            .IsRequired()
            .HasConversion(
               phoneNumber => phoneNumber.Value,
               value => PhoneNumber.Create(value).Value)
            .HasMaxLength(15);

        builder.HasIndex(user => user.PhoneNumber)
            .IsUnique()
            .HasDatabaseName("IX_User_PhoneNumber");

        builder.HasOne(user => user.Cart)
            .WithOne(cart => cart.User)
            .HasForeignKey<Cart>(cart => cart.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Accounts)
            .WithOne(account => account.User)
            .HasForeignKey(account => account.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(refreshToken => refreshToken.User)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.VerificationTokens)
            .WithOne(verificationToken => verificationToken.User)
            .HasForeignKey(verificationToken => verificationToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.Purchases)
            .WithOne(purchase => purchase.User)
            .HasForeignKey(purchase => purchase.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(user => user.Accounts)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.VerificationTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(user => user.Purchases)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}