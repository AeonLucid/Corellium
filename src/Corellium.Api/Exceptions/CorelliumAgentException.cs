namespace Corellium.Api.Exceptions;

public class CorelliumAgentException : CorelliumException
{
    public CorelliumAgentException(string? message) : base(message)
    {
    }

    public CorelliumAgentException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}