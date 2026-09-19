using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Variants.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Variants.Commands.UpdateVariant;

public sealed class UpdateVariantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
    ILogger<UpdateVariantCommandHandler> logger) : IRequestHandler<UpdateVariantCommand, Result<VariantDto>>
{
    public async Task<Result<VariantDto>> Handle(UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await unitOfWork.Variants.GetByIdAsync(request.VariantId, cancellationToken);

        if (variant is null)
        {
            logger.LogWarning( "Update variant failed: variant not found for VariantId: {VariantId}", request.VariantId);
            return ApplicationErrors.VariantNotFound;
        }

        var updateResult = variant.Update(
            request.Size ?? variant.Size,
            request.Color ?? variant.Color,
            request.StockQuantity ?? variant.StockQuantity);

        if (updateResult.IsError)
            return updateResult.Errors;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<VariantDto>(variant);
    }
}