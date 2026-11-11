using System;
using System.Runtime.Serialization;

namespace Exceptions;

/// <summary>
/// Base exception type for those are thrown by system for End User specific exceptions.
/// </summary>
[Serializable]
public class ExternalServiceUnavailableException : BusinessException
{

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    public ExternalServiceUnavailableException()
    {

    }

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    public ExternalServiceUnavailableException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public ExternalServiceUnavailableException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public ExternalServiceUnavailableException(string message,string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public ExternalServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ExternalServiceUnavailableException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="errorCode">Error Code</param>
    /// <param name="innerException">Inner exception</param>
    public ExternalServiceUnavailableException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
