using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class EmailConfirmationRequiredException : CoreException
    {
        public override string Key => "EmailConfirmationRequired";
        public override string Message => "Email confirmation is required in order to authenticate.";

        public EmailConfirmationRequiredException() : base()
        {
        }

        protected EmailConfirmationRequiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}