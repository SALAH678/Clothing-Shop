using Application.Common.Errors;
using Application.Common.Interfaces;
using Application.Features.Variants.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Variants.Commands.CreateVariant;

public sealed class CreateVariantCommandHandler(IUnitOfWork unitOfWork, IMapper mapper,
    ILogger<CreateVariantCommandHandler> logger) : IRequestHandler<CreateVariantCommand, Result<VariantDto>>
{
    public async Task<Result<VariantDto>> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
    {
        var product = await unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Create variant failed: product not found for ProductId: {ProductId}", request.ProductId);
            return ApplicationErrors.ProductsNotExists;
        }

        var variantResult = product.AddVariant(request.Size, request.Color, request.StockQuantity);

        if (variantResult.IsError)
            return variantResult.Errors;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<VariantDto>(variantResult.Value);
    }
}