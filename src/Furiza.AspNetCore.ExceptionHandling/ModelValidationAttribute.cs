using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;

namespace Furiza.AspNetCore.ExceptionHandling
{
    public class ModelValidationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid)
                return;

            var errors = new List<ModelValidationExceptionItem>();
            foreach (var value in context.ModelState.Values)
                foreach (var error in value.Errors)
                    errors.Add(new ModelValidationExceptionItem(error.ErrorMessage));

            throw new ModelValidationException(errors);
        }
    }
}