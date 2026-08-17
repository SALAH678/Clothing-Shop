using Application.Common.Interfaces;
using Application.Features.Categories.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public class GetCategoriesQueryHandler(IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IMapper _mapper = mapper;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken);
        var categoryDtos = categories.Select(c => _mapper.Map<CategoryDto>(c)).ToList();
        return categoryDtos;
    }
}
