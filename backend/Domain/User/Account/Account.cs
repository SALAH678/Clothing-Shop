using Domain.Common;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Password;

namespace Domain.Users.Accounts;

public class Account : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; } = null!;
    public string ProviderAccountId { get; private set; } = null!;
    public Password Password { get; private set; } = null!;

    public User User { get; private set; } = null!;

    protected Account()
    {
    }

    protected Account(Guid userId, string provider, string providerAccountId, Password password)
        : base(Guid.Empty)
    {
        UserId = userId;
        Provider = provider;
        ProviderAccountId = providerAccountId;
        Password = password;
    }

    public static Result<Account> Create(Guid userId, string? provider, string? providerAccountId, Password? password)
    {
        Error? error = Validate(userId, provider, providerAccountId, password);

        if (error is not null)
            return error.Value;

        return new Account(
                userId,
                provider!.Trim(),
                providerAccountId!.Trim(),
                password!);
    }

    public Result<Updated> Update(string? provider, string? providerAccountId, Password? password)
    {
        Error? error = Validate(UserId, provider, providerAccountId, password);

        if (error is not null)
            return error.Value;

        Provider = provider!.Trim();
        ProviderAccountId = providerAccountId!.Trim();
        Password = password!;

        return Result.Updated;
    }

    private static Error? Validate(Guid userId, string? provider, string? providerAccountId, Password? password)
    {
        if (userId == Guid.Empty)
            return AccountErrors.UserIdRequired;

        if (string.IsNullOrWhiteSpace(provider))
            return AccountErrors.ProviderRequired;

        if (string.IsNullOrWhiteSpace(providerAccountId))
            return AccountErrors.ProviderAccountIdRequired;

        if (password is null)
            return AccountErrors.PasswordRequired;

        return null;
    }
}
