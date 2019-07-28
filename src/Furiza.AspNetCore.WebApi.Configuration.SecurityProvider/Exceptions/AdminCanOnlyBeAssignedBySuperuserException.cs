using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApi.Configuration.SecurityProvider.Exceptions
{
    [Serializable]
    public class AdminCanOnlyBeAssignedBySuperuserException : CoreException
    {
        public override string Key => "AdminCanOnlyBeAssignedBySuperuser";
        public override string Message => "Role administrator can only be assigned by Superuser.";

        public AdminCanOnlyBeAssignedBySuperuserException() : base()
        {
        }

        protected AdminCanOnlyBeAssignedBySuperuserException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}