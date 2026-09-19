using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Features.Products.Commands.UpdateProduct;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

public sealed class UpdateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork, IMapper mapper,
    ILogger<UpdateProductCommandHandler> logger) : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
        {
            logger.LogWarning("Update failed: product {ProductId} not found", request.ProductId);
            return Error.NotFound("Product.NotFound", $"Product with id '{request.ProductId}' was not found.");
        }

        var updateResult = product.Update(
            name: request.Name,
            description: request.Description,
            basePrice: request.BasePrice,
            discount: request.Discount,
            categoryId: request.CategoryId);

        if (updateResult.IsError)
            return updateResult.TopError;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Product {ProductId} updated successfully", product.Id);

        return mapper.Map<ProductDto>(product);
    }
}