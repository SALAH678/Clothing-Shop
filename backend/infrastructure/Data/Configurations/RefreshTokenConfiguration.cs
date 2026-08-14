using Domain.Users.RefreshTokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : AuditableEntityConfiguration<RefreshToken>
{
    public override void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("RefreshTokens", schema: "identity");

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.Value)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc)
            .IsRequired();

        builder.Property(refreshToken => refreshToken.IsRevoked)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(refreshToken => refreshToken.Value)
            .IsUnique();
    }
}