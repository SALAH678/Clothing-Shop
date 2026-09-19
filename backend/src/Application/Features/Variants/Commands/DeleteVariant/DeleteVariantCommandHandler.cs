using Application.Common.Interfaces;
using Domain.Common.Results;
using Domain.Products;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Variants.Commands.DeleteVariant;

public sealed class DeleteVariantCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<DeleteVariantCommandHandler> logger)
    : IRequestHandler<DeleteVariantCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await unitOfWork.Variants.GetByIdAsync(request.VariantId, cancellationToken);

        if (variant is null)
        {
            logger.LogWarning("Delete variant failed: variant not found for VariantId: {VariantId}", request.VariantId);
            return ProductErrors.VariantNotFound;
        }

        unitOfWork.Variants.Delete(variant);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Deleted;
    }
}