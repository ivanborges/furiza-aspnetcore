using Furiza.Base.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    [Serializable]
    public class AuthException : CoreException<AuthExceptionItem>
    {
        public override string Key => "Auth";

        public AuthException(IEnumerable<AuthExceptionItem> coreExceptionItems) : base(coreExceptionItems)
        {
        }

        protected AuthException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}