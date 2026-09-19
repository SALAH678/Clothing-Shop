using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Users.Query.GetAllUsers;

public class GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetAllUsersQueryHandler> logger, IUser user)
    : IRequestHandler<GetAllUsersQuery, Result<PaginatedList<UserDto>>>
{
    private readonly ILogger<GetAllUsersQueryHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<PaginatedList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get all users requested for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        var users = await _unitOfWork.Users.GetPaginatedUsersAsync(request.PageNumber, request.PageSize, cancellationToken);
        var mapped = _mapper.Map<PaginatedList<UserDto>>(users);

        _logger.LogInformation("Get all users succeeded with Count: {Count}, Email: {Email}, UserId: {UserId}",
            mapped.Items?.Count, _user.Email, _user.UserId);

        return mapped;
    }
}