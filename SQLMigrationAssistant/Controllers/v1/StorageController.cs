using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.Application.Common.Interfaces;

namespace SQLMigrationAssistant.API.Controllers.v1
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    public class StorageController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;
        private readonly IFileStorageService _fileManagerService;


        public StorageController(ILogger<HealthController> logger, IFileStorageService fileManagerService)
        {
            _logger = logger;
            _fileManagerService = fileManagerService;
        }

        [HttpGet("testconnectivity")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Test()
        {
            return Ok(await _fileManagerService.IsServiceAvailableAsync());
        }

        [HttpPost("text")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UploadText()
        {
            var text = "Do am he horrible distance marriage so although. Afraid assure square so happen mr an before. His many same been well can high that. Forfeited did law eagerness allowance improving assurance bed. Had saw put seven joy short first. Pronounce so enjoyment my resembled in forfeited sportsman. Which vexed did began son abode short may. Interested astonished he at cultivated or me. Nor brought one invited she produce her.\r\n\r\nPicture removal detract earnest is by. Esteems met joy attempt way clothes yet demesne tedious. Replying an marianne do it an entrance advanced. Two dare say play when hold. Required bringing me material stanhill jointure is as he. Mutual indeed yet her living result matter him bed whence.\r\n\r\nBy an outlived insisted procured improved am. Paid hill fine ten now love even leaf. Supplied feelings mr of dissuade recurred no it offering honoured. Am of of in collecting devonshire favourable excellence. Her sixteen end ashamed cottage yet reached get hearing invited. Resources ourselves sweetness ye do no perfectly. Warmly warmth six one any wisdom. Family giving is pulled beauty chatty highly no. Blessing appetite domestic did mrs judgment rendered entirely. Highly indeed had garden not.";
            var filename = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + "uploadFromController.txt";
            return Ok(await _fileManagerService.UploadFileAsync(text, filename, "text/plain"));
        }
    }
}
