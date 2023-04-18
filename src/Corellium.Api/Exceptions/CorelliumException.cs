namespace Corellium.Api.Exceptions;

public class CorelliumException : Exception
{
    public CorelliumException(string? message) : base(message)
    {
    }

    public CorelliumException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}