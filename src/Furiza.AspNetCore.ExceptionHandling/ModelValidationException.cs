using Furiza.Base.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Furiza.AspNetCore.ExceptionHandling
{
    [Serializable]
    public class ModelValidationException : CoreException<ModelValidationExceptionItem>
    {
        public override string Key => "ModelValidation";
        public override string Message => $"One or more errors occurred during model validation. Please check '{nameof(Items)}' for details.";

        public ModelValidationException(IEnumerable<ModelValidationExceptionItem> coreExceptionItems) : base(coreExceptionItems)
        {
        }

        protected ModelValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}