
using Chargily.Pay.Models;

namespace Application.Common.Interfaces.Services;

public interface IPaymentGatewayService
{
    Task<CheckoutResult> CreateCheckoutAsync(Guid purchaseId, decimal amount);
    Task<CheckoutStatus> GetCheckoutStatusAsync(string checkoutId);
}

public record CheckoutResult(string CheckoutId, Uri CheckoutUrl);

