using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Commands.UpdateProduct;

public class UpdateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Error.NotFound(
                code: "Product_Not_Found",
                description: $"Product with ID {request.ProductId} was not found.");

        var updated = product.Update(
            request.Name ?? product.Name,
            request.Description ?? product.Description,
            request.BasePrice ?? product.BasePrice,
            request.Discount ?? product.Discount ?? 0,
            product.CategoryId);

        if (updated.IsError)
            return updated.Errors;

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ProductDto>(product);
        }
        catch
        {
            return Error.Failure(
                code: "Product_Update_Failed",
                description: "Product update failed.");
        }
    }
}
