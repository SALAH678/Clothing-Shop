using Domain.Users.VerificationTokens;
using Domain.Users.VerificationTokens.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations.Identify;

public class VerificationTokenConfiguration : AuditableEntityConfiguration<VerificationToken>
{
    public override void Configure(EntityTypeBuilder<VerificationToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("VerificationTokens", schema: "identity", t =>
        {
            t.HasCheckConstraint(
                name: "CK_VerificationTokens_Type",
                sql: "\"Type\" IN ('EmailVerification', 'PasswordReset')"
                );
        });

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(verificationToken => verificationToken.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(verificationToken => verificationToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(verificationToken => verificationToken.IsUsed)
            .IsRequired();

        builder.Property(verificationToken => verificationToken.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(30)
            .HasDefaultValue(VerificationTokenType.EmailVerification);
    }
}