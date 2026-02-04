using System;
using System.Net;

namespace AdvancedDevSample.Domain.Exceptions
{
    public class ApplicationServiceException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public ApplicationServiceException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public ApplicationServiceException(string message, Exception innerException, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
