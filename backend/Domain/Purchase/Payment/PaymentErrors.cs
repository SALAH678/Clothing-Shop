using Domain.Common.Results;

namespace Domain.Purchases.Payments;

public static class PaymentErrors
{
    public static Error PaymentRequired => Error.Validation(code: "Payment_Required", description: "Payment is required.");
    public static Error PurchaseIdRequired => Error.Validation(code: "Payment_Purchase_Id_Required", description: "Purchase id is required.");
    public static Error InvalidAmount => Error.Validation(code: "Payment_Invalid_Amount", description: "Payment amount cannot be negative.");
    public static Error InvalidStatus => Error.Validation(code: "Payment_Invalid_Status", description: "Payment status is invalid.");
    public static Error TransactionIdRequired => Error.Validation(code: "Payment_Transaction_Id_Required", description: "Transaction id is required.");
}
