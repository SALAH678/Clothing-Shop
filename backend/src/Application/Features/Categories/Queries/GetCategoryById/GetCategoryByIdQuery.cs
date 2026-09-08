
using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(Guid CategoryId) : IRequest<Result<CategoryDto>>;    
