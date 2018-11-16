using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class RoleDoesNotExistException : CoreException
    {
        public override string Key => "RoleDoesNotExist";
        public override string Message => "Role does not exist.";

        public RoleDoesNotExistException() : base()
        {
        }

        protected RoleDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}