using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>;
