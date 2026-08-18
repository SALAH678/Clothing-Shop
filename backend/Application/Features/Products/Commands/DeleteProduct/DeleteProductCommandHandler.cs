using Application.Common.Interfaces;
using Domain.Common.Results;
using MediatR;

namespace Application.Features.Products.Commands.DeleteProduct;

public class DeleteProductCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteProductCommand, Result<Deleted>>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Result<Deleted>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product is null)
            return Error.NotFound(
                code: "Product_Not_Found",
                description: $"Product with ID {request.ProductId} was not found.");

        _unitOfWork.Products.Delete(product);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Deleted;
        }
        catch
        {
            return Error.Failure(
                code: "Product_Delete_Failed",
                description: "Product delete failed.");
        }
    }
}
