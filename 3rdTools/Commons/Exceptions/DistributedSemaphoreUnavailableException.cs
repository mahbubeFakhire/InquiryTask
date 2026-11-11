using System;

namespace Infrastructure.Common.DistributedSemaphore.Exceptions;

public class DistributedSemaphoreUnavailableException : Exception
{
    public DistributedSemaphoreUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}