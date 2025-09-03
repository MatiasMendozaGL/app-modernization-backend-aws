using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SQLMigrationAssistant.API.Validators
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var validationProblemDetails = new ValidationProblemDetails(context.ModelState)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed",
                    Instance = context.HttpContext.Request.Path,
                    Type = "https://httpstatuses.com/400"
                };

                context.Result = new BadRequestObjectResult(validationProblemDetails);
            }
        }
    }
}
