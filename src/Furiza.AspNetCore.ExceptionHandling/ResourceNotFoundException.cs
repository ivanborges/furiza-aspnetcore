using Furiza.Base.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.ExceptionHandling
{
    [Serializable]
    public class ResourceNotFoundException : CoreException<ResourceNotFoundExceptionItem>
    {
        public override string Key => "ResourceNotFound";
        public override string Message => $"One or more resources were not found. Please check '{nameof(Items)}' for details.";

        public ResourceNotFoundException(IEnumerable<ResourceNotFoundExceptionItem> coreExceptionItems) : base(coreExceptionItems)
        {
        }

        protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}