using System;

namespace Exceptions
{
    public class NotModifiedException : Exception
    {
        public NotModifiedException()
        : base()
        {
        }

        public NotModifiedException(string message)
            : base(message)
        {
        }

        public NotModifiedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
