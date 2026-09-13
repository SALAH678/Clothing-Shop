
using Chargily.Pay.Models;

namespace Application.Common.Interfaces.Services;

public interface IPaymentGatewayService
{
    Task<CheckoutResult> CreateCheckout(Guid purchaseId, decimal amount);
    Task<CheckoutStatus> GetCheckoutStatus(string checkoutId);
}

public record CheckoutResult(string CheckoutId, Uri CheckoutUrl);

