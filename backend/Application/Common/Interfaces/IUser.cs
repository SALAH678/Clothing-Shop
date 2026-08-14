namespace Application.Common.Interfaces;

public interface IUser
{
    Guid UserId { get; }
    string? Email { get; }
    string? Role { get; }
}
