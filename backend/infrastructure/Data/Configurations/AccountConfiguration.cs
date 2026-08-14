using Domain.Common.ValueObjects.Password;
using Domain.Users.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations;

public class AccountConfiguration : AuditableEntityConfiguration<Account>
{
    public override void Configure(EntityTypeBuilder<Account> builder)
    {
        base.Configure(builder);

        builder.ToTable("Accounts", schema: "Identity");

        builder.Property(c => c.UserId)
                .IsRequired();

        builder.Property(account => account.Provider)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(account => account.ProviderAccountId)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(account => account.Password)
            .IsRequired(false)
            .HasConversion(
            password => password!.Value,
            value => Password.Create(value).Value)
            .HasMaxLength(150)
            .HasDefaultValue(null);

        builder.HasIndex(account => new { account.Provider, account.ProviderAccountId })
            .IsUnique();
    }
}