using Application.Features.Categories.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string CategoryName,
    Stream? ImageContent,
    string? ImageFileName) : IRequest<Result<CategoryDto>>;
