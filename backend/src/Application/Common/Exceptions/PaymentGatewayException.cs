namespace Application.Common.Exceptions;

public class PaymentGatewayException : Exception
{
    public PaymentGatewayException(string message) : base(message) { }
    public PaymentGatewayException(string message, Exception inner) : base(message, inner) { }
}
