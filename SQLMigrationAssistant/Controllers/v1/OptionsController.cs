using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.API.Controllers.v1
{
    [Authorize]
    [Route("api/v1/[controller]")]
    public class OptionsController : BaseController
    {

        public OptionsController(IMediator mediator, ILogger<OptionsController> logger)
            : base(mediator, logger)
        {
        }

        [HttpGet("llm-providers")]
        [ProducesResponseType(typeof(IEnumerable<OptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLLMProviders()
        {
            var result = await Mediator.Send(new LLMProviderRequest());
            return Ok(result);
        }

        [HttpGet("target-languages")]
        [ProducesResponseType(typeof(IEnumerable<OptionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTargetLanguages()
        {
            var result = await Mediator.Send(new TargetLanguageRequest());
            return Ok(result);
        }
    }
}
