using SharedModels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Text.Json;

namespace Exceptions
{
    /// <summary>
    /// Base exception type for those are thrown by system for client side specific exceptions.
    /// </summary>
    [Serializable]
    public class HttpHandlerException : Exception, IJosnCustomConvertor
    {
        public ProblemDetails ProblemDetails { get; }
        public List<string> Errors { get; }
        public HttpStatusCode HttpStatusCode { get; }
        public string ErrorCode { get; set; }

        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        public HttpHandlerException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        public HttpHandlerException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public HttpHandlerException(string message, string errorCode)
            : base(message)
        {
            Errors = new List<string>() { message };
            ErrorCode = errorCode;
        }

        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public HttpHandlerException(string message, string errorCode, Exception innerException)
            : base(message, innerException)
        {
            Errors = new List<string>() { message };
            ErrorCode = errorCode;
        }
        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        /// <param name="problemDetails"></param>
        public HttpHandlerException(ProblemDetails problemDetails, string errorCode)
           : base(problemDetails.Detail)
        {
            ProblemDetails = problemDetails;
            ErrorCode = errorCode;
        }
        /// <summary>
        /// Creates a new <see cref="ClientSideException"/> object.
        /// </summary>
        /// <param name="problemDetails"></param>
        /// <param name="error"></param>
        public HttpHandlerException(ProblemDetails problemDetails, List<string> errors, HttpStatusCode httpStatusCode, string errorCode)
           : base(problemDetails?.Detail)
        {
            ProblemDetails = problemDetails;
            Errors = errors;
            HttpStatusCode = httpStatusCode;
            ErrorCode = errorCode;
        }

        public bool IsBadRequest => HttpStatusCode == HttpStatusCode.BadRequest;
        public bool IsChannelException => ProblemDetails?.Status == 453;

        public Action<Utf8JsonWriter> ExtraPropertyHandler => (Utf8JsonWriter writer) =>
        {
            var httpHandlerException = this;
            writer.WriteString("Errors", JsonSerializer.Serialize(httpHandlerException.Errors));
            writer.WriteString("ProblemDetails", JsonSerializer.Serialize(httpHandlerException.ProblemDetails));
            writer.WriteString("HttpStatusCode", httpHandlerException.HttpStatusCode.ToString());
        };
    }
}
