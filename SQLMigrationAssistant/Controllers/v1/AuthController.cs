using MediatR;
using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.API.Controllers.v1
{
    [Route("api/v1/[controller]")]
    public class AuthController : BaseController
    {
        public AuthController(IMediator mediator, ILogger<AuthController> logger)
            : base(mediator, logger)
        {
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var response = await Mediator.Send(request);
            Logger.LogInformation("Login successful for user: {Email}", request.Email);
            return Ok(response);
        }
    }
}
