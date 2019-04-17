using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    [Serializable]
    public class RoleAlreadyExistsException : CoreException
    {
        public override string Key => "RoleAlreadyExists";
        public override string Message => "Rolename already exists.";

        public RoleAlreadyExistsException() : base()
        {
        }

        protected RoleAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}