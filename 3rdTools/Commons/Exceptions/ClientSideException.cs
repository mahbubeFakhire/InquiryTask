using SharedModels;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Exceptions;

/// <summary>
/// Base exception type for those are thrown by system for client side specific exceptions.
/// </summary>
[Serializable]
public class ClientSideException : Exception
{
    public ProblemDetails ProblemDetails { get; }
    public List<string> Errors { get; }

    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    public ClientSideException()
    {

    }

    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    public ClientSideException(SerializationInfo serializationInfo, StreamingContext context)
        : base(serializationInfo, context)
    {

    }

    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    public ClientSideException(string message)
        : base(message)
    {
        Errors = new List<string>() { message };
    }

    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    /// <param name="message">Exception message</param>
    /// <param name="innerException">Inner exception</param>
    public ClientSideException(string message, Exception innerException)
        : base(message, innerException)
    {
        Errors = new List<string>() { message };
    }
    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    /// <param name="problemDetails"></param>
    public ClientSideException(ProblemDetails problemDetails)
       : base(problemDetails.Detail)
    {
        ProblemDetails = problemDetails;
    }
    /// <summary>
    /// Creates a new <see cref="ClientSideException"/> object.
    /// </summary>
    /// <param name="problemDetails"></param>
    /// <param name="error"></param>
    public ClientSideException(ProblemDetails problemDetails, List<string> errors)
       : base(problemDetails.Detail)
    {
        ProblemDetails = problemDetails;
        Errors = errors;
    }
}
