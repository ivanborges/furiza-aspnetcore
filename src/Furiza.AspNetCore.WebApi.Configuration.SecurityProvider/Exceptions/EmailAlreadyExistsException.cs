using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    [Serializable]
    public class EmailAlreadyExistsException : CoreException
    {
        public override string Key => "EmailAlreadyExists";
        public override string Message => "Email already exists.";

        public EmailAlreadyExistsException() : base()
        {
        }

        protected EmailAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}