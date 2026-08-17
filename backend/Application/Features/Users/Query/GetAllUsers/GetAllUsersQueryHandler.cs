using Application.Common.Interfaces;
using Application.Features.Users.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Users.Query.GetAllUsers;

public class GetAllUsersQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GetAllUsersQuery, Result<List<UserDto>>>
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<List<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        var userDtos = users.Select(u => _mapper.Map<UserDto>(u)).ToList();
        return userDtos;
    }
}