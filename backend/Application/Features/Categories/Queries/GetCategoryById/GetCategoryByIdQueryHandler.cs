using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;
namespace Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper, ILogger<GetCategoryByIdQueryHandler> logger, IUser user) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly ILogger<GetCategoryByIdQueryHandler> _logger = logger;
    private readonly IUser _user = user;

    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Get category requested for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
            request.CategoryId, _user.Email, _user.UserId);

        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            _logger.LogWarning("Get category failed: category not found for CategoryId: {CategoryId}, Email: {Email}, UserId: {UserId}",
                request.CategoryId, _user.Email, _user.UserId);
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.CategoryId} was not found."
            );

        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return categoryDto;
    }
}
