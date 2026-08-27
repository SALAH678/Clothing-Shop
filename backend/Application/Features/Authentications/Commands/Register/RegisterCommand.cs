using Domain.Common.Results;
using MediatR;

namespace Application.Features.Authentications.Command.Register;

public sealed record RegisterCommand(
     string FirstName,
     string LastName,
     string PhoneNumber,
     string Email,
     string Password) : IRequest<Result<string>>;
