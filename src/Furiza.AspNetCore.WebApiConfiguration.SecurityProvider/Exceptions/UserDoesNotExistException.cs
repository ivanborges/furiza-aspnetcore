using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class UserDoesNotExistException : CoreException
    {
        public override string Key => "UserDoesNotExist";
        public override string Message => "Username does not exist.";

        public UserDoesNotExistException() : base()
        {
        }

        protected UserDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}