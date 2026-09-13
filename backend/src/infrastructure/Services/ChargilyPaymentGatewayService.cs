using Application.Common.Exceptions;
using Application.Common.Interfaces.Services;
using Chargily.Pay.Abstractions;
using Chargily.Pay.Models;
using infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace infrastructure.Services;

public sealed class ChargilyPaymentGatewayService(IChargilyPayClient client, IOptions<ChargilyOptions> options,
    ILogger<ChargilyPaymentGatewayService> logger) : IPaymentGatewayService
{
    private readonly IChargilyPayClient _client = client;
    private readonly ChargilyOptions _options = options.Value;
    private readonly ILogger<ChargilyPaymentGatewayService> _logger = logger;

    public async Task<CheckoutResult> CreateCheckout(Guid purchaseId, decimal amount)
    {
        _logger.LogInformation("Requesting Chargily checkout for purchase {PurchaseId}, amount {Amount}, live mode {IsLiveMode}", purchaseId, amount,
            _options.IsLiveMode);

        var checkout = new Checkout(amount: amount, currency: Currency.DZD)
        {
            Description = $"Order {purchaseId}",
            Language = LocaleType.English,
            WebhookEndpointUrl = new Uri(_options.WebhookEndpointUrl),
            OnSuccessRedirectUrl = new Uri($"{_options.SuccessRedirectBaseUrl}?purchaseId={purchaseId}"),
            OnFailureRedirectUrl = new Uri($"{_options.FailureRedirectBaseUrl}?purchaseId={purchaseId}"),
            //Metadata = new Dictionary<string, string> { ["purchaseId"] = purchaseId.ToString() }
            Metadata = [purchaseId.ToString()]
        };

        Response<CheckoutResponse> result;

        try
        {
            result = await _client.CreateCheckout(checkout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Chargily CreateCheckout for purchase {PurchaseId}", purchaseId);
            throw new PaymentGatewayException("Payment gateway is unavailable.", ex);
        }

        _logger.LogInformation( "Chargily checkout {CheckoutId} created successfully for purchase {PurchaseId}", result.Value.Id, purchaseId);

        return new CheckoutResult(result.Value.Id, result.Value.CheckoutUrl!);
    }

    public async Task<CheckoutStatus> GetCheckoutStatus(string checkoutId)
    {
        _logger.LogInformation("Fetching Chargily checkout status for {CheckoutId}", checkoutId);

        Response<CheckoutResponse>? result;

        try
        {
            result = await _client.GetCheckout(checkoutId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Chargily GetCheckout for {CheckoutId}", checkoutId);
            throw new PaymentGatewayException($"Could not retrieve checkout status for {checkoutId}.", ex);
        }

        if (result is null)
        {
            _logger.LogWarning("Chargily returned no checkout for {CheckoutId}", checkoutId);
            throw new PaymentGatewayException($"Could not retrieve checkout status for {checkoutId}.");
        }

        _logger.LogInformation("Chargily checkout {CheckoutId} status: {Status}", checkoutId, result.Value.Status);

        return result.Value.Status;
    }
}
