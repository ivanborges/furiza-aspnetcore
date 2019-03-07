using Furiza.Base.Core.Exceptions;
using System;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    [Serializable]
    public class DefaultRoleViolatedException : CoreException
    {
        public override string Key => "DefaultRoleViolated";
        public override string Message => "Default roles can not be modified.";

        public DefaultRoleViolatedException() : base()
        {
        }

        protected DefaultRoleViolatedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}