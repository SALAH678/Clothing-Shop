using Domain.Common;
using Domain.Common.Results;
using Domain.Users.VerificationTokens.Enum;

namespace Domain.Users.VerificationTokens;

public class VerificationToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Code { get; private set; } = null!;
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public bool IsUsed { get; private set; }
    public VerificationTokenType Type { get; private set; }

    public User User { get; private set; } = null!;

    protected VerificationToken()
    {
    }

    protected VerificationToken(Guid userId, string code, DateTimeOffset expiresAtUtc, VerificationTokenType type)
        : base(Guid.Empty)
    {
        UserId = userId;
        Code = code;
        ExpiresAtUtc = expiresAtUtc;
        Type = type;
        IsUsed = false;
    }

    public static Result<VerificationToken> Create(Guid userId, string? code, DateTimeOffset expiresAtUtc, VerificationTokenType? type)
    {
        Error? error = Validate(userId, code, expiresAtUtc, type);

        if (error is not null)
            return error.Value;

        return new VerificationToken(userId, code!.Trim(), expiresAtUtc, type!.Value);
    }

    public Result<Success> Verify(string code)
    {
        if (IsUsed)
            return VerificationTokenErrors.CodeAlreadyUsed;

        if (DateTimeOffset.UtcNow > ExpiresAtUtc)
            return VerificationTokenErrors.CodeExpired;

        if (Code != code)
            return VerificationTokenErrors.InvalidOrExpiredCode;

        return Result.Success;
    }

    public Result<Updated> MarkAsUsed()
    {
        if (IsUsed)
            return VerificationTokenErrors.VerificationTokenAlreadyUsed;

        IsUsed = true;

        return Result.Updated;
    }

    public Result<Updated> UpdateExpiration(DateTimeOffset expiresAtUtc)
    {
        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            return VerificationTokenErrors.ExpiresAtUtcMustBeInFuture;

        ExpiresAtUtc = expiresAtUtc;

        return Result.Updated;
    }

    private static Error? Validate(Guid userId, string? code, DateTimeOffset expiresAtUtc, VerificationTokenType? type)
    {
        if (userId == Guid.Empty)
            return VerificationTokenErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(code))
            return VerificationTokenErrors.CodeRequired;

        if (expiresAtUtc <= DateTimeOffset.UtcNow)
            return VerificationTokenErrors.ExpiresAtUtcMustBeInFuture;

        if (type is null)
            return VerificationTokenErrors.TypeRequired;

        if (!System.Enum.IsDefined(type.Value))
            return VerificationTokenErrors.InvalidType;

        return null;
    }
}
