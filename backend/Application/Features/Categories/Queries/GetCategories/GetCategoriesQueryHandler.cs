using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper, ILogger<GetCategoriesQueryHandler> logger, IUser user) : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly ILogger<GetCategoriesQueryHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get categories requested for Email: {Email}, UserId: {UserId}", _user.Email, _user.UserId);

        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var categoryDtos = categories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();

        _logger.LogInformation("Get categories succeeded with Count: {Count}, Email: {Email}, UserId: {UserId}",
            categoryDtos.Count, _user.Email, _user.UserId);

        return categoryDtos;
    }
}
