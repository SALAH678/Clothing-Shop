using Application.Features.Variants.Dtos;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Variants.Commands.CreateVariant;

public sealed record CreateVariantCommand(
    Guid ProductId,
    string Size,
    string Color,
    int StockQuantity) : IRequest<Result<VariantDto>>;