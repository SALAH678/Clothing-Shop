using Domain.Common;
using Domain.Common.Results;

namespace Domain.Users.RefreshTokens;

public class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Value { get; private set; } = null!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public bool IsRevoked { get; private set; }

    public User User { get; private set; } = null!;

    protected RefreshToken()
    {
    }

    protected RefreshToken(Guid userId, string value, DateTimeOffset expiresAtUtc)
        : base(Guid.Empty)
    {
        UserId = userId;
        Value = value;
        ExpiresAtUtc = expiresAtUtc;
        IsRevoked = false;
    }

    public static Result<RefreshToken> Create(Guid userId, string? value, DateTimeOffset expiresAtUtc)
    {
        Error? error = Validate(userId, value, expiresAtUtc);

        if (error is not null)
            return error.Value;

        return new RefreshToken(userId, value!.Trim(), expiresAtUtc);
    }

    public Result<Updated> Revoke()
    {
        if (IsRevoked)
            return RefreshTokenErrors.RefreshTokenAlreadyRevoked;

        IsRevoked = true;

        return Result.Updated;
    }

    public Result<Updated> UpdateExpiration(DateTimeOffset expiresAtUtc)
    {
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            return RefreshTokenErrors.ExpiresAtUtcMustBeInFuture;

        ExpiresAtUtc = expiresAtUtc;

        return Result.Updated;
    }

    private static Error? Validate(Guid userId, string? value, DateTimeOffset expiresAtUtc)
    {
        if (userId == Guid.Empty)
            return RefreshTokenErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(value))
            return RefreshTokenErrors.ValueRequired;

        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            return RefreshTokenErrors.ExpiresAtUtcMustBeInFuture;

        return null;
    }
}
