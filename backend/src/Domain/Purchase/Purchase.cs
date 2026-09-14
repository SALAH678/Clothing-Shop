using Domain.Common;
using Domain.Common.Results;
using Domain.Common.ValueObjects.Address;
using Domain.Common.ValueObjects.PhoneNumber;
using Domain.Purchases.Enum;
using Domain.Purchases.Payments;
using Domain.Purchases.Payments.Enum;
using Domain.Purchases.PurchaseItems;
using Domain.Users;

namespace Domain.Purchases;

public class Purchase : AuditableEntity
{
    public Guid UserId { get; private set; }
    public PhoneNumber CustomerPhone { get; private set; } = null!;
    public Address CustomerAddress { get; private set; } = null!;
    public decimal TotalAmount { get; private set; }
    public PurchaseOrigin Origin { get; private set; }

    private readonly List<PurchaseItem> _items = [];

    public IReadOnlyCollection<PurchaseItem> Items => _items.AsReadOnly();
    public Payment Payment { get; private set; } = null!;
    public User User { get; private set; } = null!;

    protected Purchase()
    {
    }

    protected Purchase(Guid userId, PhoneNumber customerPhone, Address customerAddress, PurchaseOrigin origin)
        : base(Guid.Empty)
    {
        UserId = userId;
        CustomerPhone = customerPhone;
        CustomerAddress = customerAddress;
        TotalAmount = 0;
        Origin = origin;
    }

    public static Result<Purchase> Create(Guid userId, PhoneNumber customerPhone, Address customerAddress, PurchaseOrigin origin)
    {
        Error? error = Validate(userId, customerPhone, customerAddress);

        if (error is not null)
            return error.Value;

        return new Purchase(userId, customerPhone, customerAddress, origin);
    }

    public Result<Updated> UpdateCustomerInfo(PhoneNumber customerPhone, Address customerAddress)
    {
        Error? error = Validate(UserId, customerPhone, customerAddress);

        if (error is not null)
            return error.Value;

        CustomerPhone = customerPhone!;
        CustomerAddress = customerAddress!;

        return Result.Updated;
    }

    public Result<PurchaseItem> AddItem(Guid variantId, int quantity, decimal unitPrice)
    {
        PurchaseItem? existingItem = _items.FirstOrDefault(item => item.VariantId == variantId);

        if (existingItem is not null)
        {
            Result<Updated> updateResult = existingItem.IncreaseQuantity(quantity);

            if (updateResult.IsError)
                return updateResult.Errors;

            RecalculateTotalAmount();

            return existingItem;
        }

        Result<PurchaseItem> itemResult = PurchaseItem.Create(Id, variantId, quantity, unitPrice);

        if (itemResult.IsError)
            return itemResult.Errors;

        _items.Add(itemResult.Value);
        RecalculateTotalAmount();

        return itemResult.Value;
    }

    public Result<Updated> UpdateItemQuantity(Guid variantId, int quantity)
    {
        PurchaseItem? item = _items.FirstOrDefault(item => item.VariantId == variantId);

        if (item is null)
            return PurchaseErrors.ItemNotFound;

        Result<Updated> updateResult = item.UpdateQuantity(quantity);

        if (updateResult.IsError)
            return updateResult.Errors;

        RecalculateTotalAmount();

        return Result.Updated;
    }

    public Result<Deleted> RemoveItem(Guid variantId)
    {
        PurchaseItem? item = _items.FirstOrDefault(item => item.VariantId == variantId);

        if (item is null)
            return PurchaseErrors.ItemNotFound;

        _items.Remove(item);
        RecalculateTotalAmount();

        return Result.Deleted;
    }

    public Result<Payment> AddPayment(decimal amount, PaymentStatus status, string? transactionId)
    {
        if (_items.Count == 0)
            return PurchaseErrors.NoItems;

        if (Payment is not null)
            return PurchaseErrors.PaymentAlreadyExists;

        Result<Payment> paymentResult = Payment.Create(Id, amount, status, transactionId);

        if (paymentResult.IsError)
            return paymentResult.Errors;

        if (amount != TotalAmount)
            return PurchaseErrors.PaymentAmountMustEqualTotalAmount;

        Payment = paymentResult.Value;

        return paymentResult.Value;
    }

    private void RecalculateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.Quantity * item.UnitPrice);
    }

    private static Error? Validate(Guid userId, PhoneNumber customerPhone, Address customerAddress)
    {
        if (userId == Guid.Empty)
            return PurchaseErrors.UserIdRequired;

        if (customerPhone is null)
            return PurchaseErrors.CustomerPhoneRequired;

        if (customerAddress is null)
            return PurchaseErrors.CustomerAddressRequired;

        return null;
    }
}
