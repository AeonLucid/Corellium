using System.Runtime.Serialization;

namespace Corellium.Api.Exceptions;

public class CorelliumException : Exception
{
    public CorelliumException()
    {
    }

    protected CorelliumException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public CorelliumException(string? message) : base(message)
    {
    }

    public CorelliumException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}