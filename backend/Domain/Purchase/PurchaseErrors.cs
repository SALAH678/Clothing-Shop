using Domain.Common.Results;

namespace Domain.Purchases;

public static class PurchaseErrors
{
    public static Error PurchaseRequired => Error.Validation(code: "Purchase_Required", description: "Purchase is required.");
    public static Error UserIdRequired => Error.Validation(code: "Purchase_User_Id_Required", description: "User id is required.");
    public static Error CustomerPhoneRequired => Error.Validation(code: "Purchase_Customer_Phone_Required", description: "Customer phone is required.");
    public static Error CustomerAddressRequired => Error.Validation(code: "Purchase_Customer_Address_Required", description: "Customer address is required.");
    public static Error NoItems => Error.Validation(code: "Purchase_No_Items", description: "Purchase must have at least one item.");
    public static Error ItemNotFound => Error.NotFound(code: "Purchase_Item_Not_Found", description: "Purchase item was not found.");
    public static Error PaymentAlreadyExists => Error.Conflict(code: "Purchase_Payment_Already_Exists", description: "Purchase already has a payment.");
    public static Error PaymentAmountMustEqualTotalAmount => Error.Validation(code: "Purchase_Payment_Amount_Invalid", description: "Payment amount must equal purchase total amount.");
}
