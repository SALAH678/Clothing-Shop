using Application.Features.Variants.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Variants.Commands.UpdateVariant;

public sealed record UpdateVariantCommand(
    Guid VariantId,
    string? Size,
    string? Color,
    int? StockQuantity) : IRequest<Result<VariantDto>>;