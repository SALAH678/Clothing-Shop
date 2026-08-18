using Application.Common.Interfaces;
using Application.Features.Products.Dtos;
using AutoMapper;
using Domain.Common.Results;
using Domain.Products;
using MediatR;

namespace Application.Features.Products.Commands.CreateProduct;

public class CreateProductCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    : IRequestHandler<CreateProductCommand, Result<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ProductDto>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var createResult = Product.Create(request.Name, request.Description, request.BasePrice, request.Discount, request.CategoryId);

        if (createResult.IsError)
            return createResult.Errors;

        if (request.Variants is not null)
        {
            foreach (var variant in request.Variants)
            {
                var variantResult = createResult.Value.AddVariant(variant.Size, variant.Color, variant.StockQuantity);
                if (variantResult.IsError)
                    return variantResult.Errors;
            }
        }

        _unitOfWork.Products.Create(createResult.Value);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<ProductDto>(createResult.Value);
        }
        catch
        {
            return Error.Failure(
                code: "Product_Creation_Failed",
                description: "Product creation failed.");
        }
    }
}
