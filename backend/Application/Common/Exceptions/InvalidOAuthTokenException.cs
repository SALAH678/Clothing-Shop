namespace Application.Common.Exceptions;

public sealed class InvalidOAuthTokenException : Exception
{
    public InvalidOAuthTokenException(string message, Exception innerException)
        : base(message, innerException) { }
}
