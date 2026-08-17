using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryDto>>
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken);

        if (category is null)
        {
            return Error.NotFound(
                code: "Category_NotFound",
                description: $"Category with ID {request.CategoryId} was not found."
            );

        }

        var categoryDto = _mapper.Map<CategoryDto>(category);
        return categoryDto;
    }
}
