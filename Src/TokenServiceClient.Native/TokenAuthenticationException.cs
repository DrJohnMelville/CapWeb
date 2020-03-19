using System;
using System.Runtime.Serialization;

namespace TokenServiceClient.Native
{
    public class TokenAuthenticationException: Exception
    {
        public TokenAuthenticationException()
        {
        }

        protected TokenAuthenticationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public TokenAuthenticationException(string message) : base(message)
        {
        }

        public TokenAuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}