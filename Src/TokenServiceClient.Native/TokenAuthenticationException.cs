using System;
using System.Runtime.Serialization;

namespace TokenServiceClient.Native
{
    public class TokenAuthenticationException: Exception
    {
        public TokenAuthenticationException()
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