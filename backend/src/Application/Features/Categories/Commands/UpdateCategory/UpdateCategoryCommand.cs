using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid categoryId,
    string? CategoryName,
    Stream? NewImageContent,
    string? ImageFileName) : IRequest<Result<CategoryDto>>;

