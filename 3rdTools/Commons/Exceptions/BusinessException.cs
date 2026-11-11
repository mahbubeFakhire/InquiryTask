using System;
using System.Runtime.Serialization;

namespace Exceptions;

/// <summary>
/// Base exception type for those are thrown by system for End User specific exceptions.
/// </summary>
[Serializable]
public class BusinessException : Exception
{
    public string ErrorCode { get; set; }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    public BusinessException()
    {

    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    public BusinessException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {

    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public BusinessException(string message)
        : base(message)
    {

    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public BusinessException(string message,string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {

    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="errorCode">Error Code</param>
    /// <param name="innerException">Inner exception</param>
    public BusinessException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
