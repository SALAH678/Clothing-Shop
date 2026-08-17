using Domain.Common.Results;
using MediatR;

namespace Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid categoryId) : IRequest<Result<Deleted>>;   
