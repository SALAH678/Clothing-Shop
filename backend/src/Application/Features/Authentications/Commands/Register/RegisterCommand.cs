using Application.Common.Attributes;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Register;

public sealed record RegisterCommand(
     string FirstName,
     string LastName,
     [property: Sensitive] string PhoneNumber,
     string Email,
     [property: Sensitive] string Password,
     string? Role) : IRequest<Result<string>>;
