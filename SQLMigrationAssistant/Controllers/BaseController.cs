using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SQLMigrationAssistant.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        protected readonly IMediator Mediator;
        protected readonly ILogger Logger;

        protected BaseController(IMediator mediator, ILogger logger)
        {
            Mediator = mediator;
            Logger = logger;
        }

        protected string CurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return idClaim?.Value ?? throw new UnauthorizedAccessException("Unable to retrieve user ID from context");
        }

        protected string CurrentUserEmail()
        {
            var emailClaim = User.FindFirst(ClaimTypes.Email);
            return emailClaim?.Value ?? throw new UnauthorizedAccessException("Unable to retrieve user Email from context");
        }
    }
}
