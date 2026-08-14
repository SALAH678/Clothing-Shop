using Domain.Common;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users.Accounts;
using Domain.Users.Enum;
using Domain.Users.RefreshTokens;
using Domain.Users.VerificationTokens;
using System.Text.RegularExpressions;

namespace Domain.Users;

public class User : AuditableEntity
{
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public Email Email { get; private set; } = null!;
    public PhoneNumber PhoneNumber { get; private set; } = null!;
    public bool EmailVerified { get; private set; }
    public Role UserRole { get; private set; }

    private readonly List<Account> _accounts = [];
    private readonly List<RefreshToken>? _refreshTokens = [];
    private readonly List<VerificationToken>? _verificationTokens = [];

    public IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();
    public IReadOnlyCollection<RefreshToken>? RefreshTokens => _refreshTokens?.AsReadOnly();
    public IReadOnlyCollection<VerificationToken>? VerificationTokens => _verificationTokens?.AsReadOnly();

    protected User()
    {
    }

    protected User(string firstName, string lastName, Email email, PhoneNumber phoneNumber, Role role)
        : base(Guid.Empty)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        UserRole = role;
        EmailVerified = false;
    }

    public static Result<User> Create(string? firstName, string? lastName, Email? email, PhoneNumber? phoneNumber, Role? role = Role.Customer)
    {
        Error? error = Validate(firstName, lastName, email, phoneNumber, role);

        if (error is not null)
            return error.Value;

        return new User(
            firstName!.Trim(),
            lastName!.Trim(),
            email!,
            phoneNumber!,
            role!.Value);
    }

    public Result<Updated> Update(string? firstName, string? lastName, Email? email, PhoneNumber? phoneNumber)
    {
        Error? error = Validate(firstName, lastName, email, phoneNumber, UserRole);

        if (error is not null)
            return error.Value;

        FirstName = firstName!.Trim();
        LastName = lastName!.Trim();
        Email = email!;
        PhoneNumber = phoneNumber!;

        return Result.Updated;
    }

    public Result<Account> AddAccount(Account account)
    {
        if (account is null)
            return AccountErrors.AccountRequired;

        _accounts.Add(account);

        return account;
    }

    public Result<RefreshToken> AddRefreshToken(RefreshToken refreshToken)
    {
        if (refreshToken is null)
            return RefreshTokenErrors.RefreshTokenRequired;

        _refreshTokens?.Add(refreshToken);

        return refreshToken;
    }

    public Result<VerificationToken> AddVerificationToken(VerificationToken verificationToken)
    {
        if (verificationToken is null)
            return VerificationTokenErrors.VerificationTokenRequired;

        _verificationTokens?.Add(verificationToken);

        return verificationToken;
    }

    public Result<Updated> VerifyEmail()
    {
        EmailVerified = true;

        return Result.Updated;
    }

    public Result<Updated> SetRole(Role newRole)
    {
        if (!System.Enum.IsDefined(newRole))
            return UserErrors.InvalidRole;

        UserRole = newRole;

        return Result.Updated;
    }

    private static Error? Validate(string? firstName, string? lastName, Email? email, PhoneNumber? phoneNumber, Role? role)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return UserErrors.FirstNameRequired;

        if (!Regex.IsMatch(firstName, @"^[a-zA-Z]+$"))
            return UserErrors.InvalidFirstName;

        if (string.IsNullOrWhiteSpace(lastName))
            return UserErrors.LastNameRequired;

        if (!Regex.IsMatch(lastName, @"^[a-zA-Z]+$"))
            return UserErrors.InvalidLastName;

        if (email is null)
            return UserErrors.EmailRequired;

        if (phoneNumber is null)
            return UserErrors.PhoneNumberRequired;

        if (role is null)
            return UserErrors.RoleRequired;

        if (!System.Enum.IsDefined(role.Value))
            return UserErrors.InvalidRole;

        return null;
    }
}
