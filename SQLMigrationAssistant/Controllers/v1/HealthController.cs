using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.Application.Common.Interfaces;

namespace SQLMigrationAssistant.API.Controllers.v1
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IFileStorageService _fileManagerService;


        public HealthController(ILogger<HealthController> logger, IFileStorageService fileManagerService)
        {
            _logger = logger;
            _fileManagerService = fileManagerService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            _logger.LogInformation("Health check received. API is alive.");
            return Ok("Healthy");
        }
    }
}
