using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    [Serializable]
    public class RoleInUseException : CoreException
    {
        public override string Key => "RoleInUse";
        public override string Message => "Rolename is in use by some role assignment or scoped role assignment.";

        public RoleInUseException() : base()
        {
        }

        protected RoleInUseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}