using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Email;
using Domain.Common.ValueObjects.Password;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Users;
using Domain.Users.Accounts;
using Domain.Users.Enum;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Command.CreateUser;

public class CreateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
    ILogger<CreateUserCommandHandler> logger) : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CreateUserCommandHandler> _logger = logger;

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Create user requested for Email: {Email}", request.Email);

        var emailResult = Email.Create(request.Email);
        if (!emailResult.IsSuccess)
            return emailResult.TopError;

        var phoneNumberResult = PhoneNumber.Create(request.PhoneNumber);
        if (!phoneNumberResult.IsSuccess)
            return phoneNumberResult.TopError;

        var emailExists = await _unitOfWork.Users.ExistsAsync(emailResult.Value, cancellationToken);
        if (emailExists)
        {
            _logger.LogWarning("Create user failed: email already exists for Email: {Email}", request.Email);
            return ApplicationErrors.UserAlreadyExists;
        }

        var parsedRole = Enum.TryParse<Role>(request.Role, ignoreCase: true, out var role) ? role : Role.Customer;

        var userResult = User.Create(request.FirstName, request.LastName, emailResult.Value, phoneNumberResult.Value, parsedRole);

        if (!userResult.IsSuccess)
        {
            _logger.LogWarning("Create user failed for Email: {Email}", request.Email);
            return userResult.TopError;
        }

        userResult.Value.MarkEmailVerified();

        var passwordResult = Password.Create(request.Password);
        if(!passwordResult.IsSuccess)
        {
            _logger.LogWarning("Create user failed: invalid password for Email: {Email}", request.Email);
            return passwordResult.TopError;
        }

        var accountResult = Account.Create(userResult.Value.Id, "local", null, passwordResult.Value);
        if(accountResult.IsError)
        {
            _logger.LogWarning("Create user failed: account creation failed for Email: {Email}", request.Email);
            return accountResult.TopError;
        }

        _unitOfWork.Users.Create(userResult.Value);
        _unitOfWork.Accounts.Create(accountResult.Value);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User created successfully for Email: {Email}, UserId: {UserId}", request.Email, userResult.Value.Id);

        return _mapper.Map<UserDto>(userResult.Value);
    }
}
