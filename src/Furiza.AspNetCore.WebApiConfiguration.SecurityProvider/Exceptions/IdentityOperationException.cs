using Furiza.Base.Core.Exceptions;
using System.Collections.Generic;

namespace Furiza.AspNetCore.WebApiConfiguration.SecurityProvider.Exceptions
{
    public class IdentityOperationException : CoreException<IdentityOperationExceptionItem>
    {
        public override string Key => "IdentityOperation";

        public override string Message => $"One or more errors occurred during an Identity operation. Please check '{nameof(Items)}' for details.";

        public IdentityOperationException(IEnumerable<IdentityOperationExceptionItem> coreExceptionItems) : base(coreExceptionItems)
        {
        }
    }
}