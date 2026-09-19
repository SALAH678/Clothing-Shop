using Domain.Common.Results;
using MediatR;

namespace Application.Features.Variants.Commands.DeleteVariant;

public sealed record DeleteVariantCommand(Guid VariantId) : IRequest<Result<Deleted>>;