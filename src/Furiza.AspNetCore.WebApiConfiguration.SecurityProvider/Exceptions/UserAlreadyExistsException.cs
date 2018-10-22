using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class UserAlreadyExistsException : CoreException
    {
        public override string Key => "UserAlreadyExists";
        public override string Message => "Username already exists.";

        public UserAlreadyExistsException() : base()
        {
        }

        protected UserAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}