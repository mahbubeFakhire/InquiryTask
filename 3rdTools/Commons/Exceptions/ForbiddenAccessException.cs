using System;
using System.Runtime.Serialization;

namespace Exceptions;

public class ForbiddenAccessException : Exception
{
    /// <summary>
    /// Creates a new <see cref="ForbiddenAccessException"/> object.
    /// </summary>
    public ForbiddenAccessException()
    {

    }

    /// <summary>
    /// Creates a new <see cref="ForbiddenAccessException"/> object.
    /// </summary>
    public ForbiddenAccessException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ForbiddenAccessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public ForbiddenAccessException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ForbiddenAccessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public ForbiddenAccessException(string message, Exception innerException)
        : base(message, innerException)
    {

    }

}
