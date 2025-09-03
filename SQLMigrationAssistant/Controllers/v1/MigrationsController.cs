using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SQLMigrationAssistant.API.Models;
using SQLMigrationAssistant.Application.DTOs;

namespace SQLMigrationAssistant.API.Controllers.v1
{
    [Authorize]
    [Route("api/v1/[controller]")]    
    public class MigrationsController : BaseController
    {
        private readonly IMapper _mapper;

        public MigrationsController(IMediator mediator, ILogger<MigrationsController> logger, IMapper mapper)
            :base(mediator, logger)
        {
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MigrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllMigrations()
        {
            Logger.LogInformation("Request to get list of migrations");
            var migrations = await Mediator.Send(new MigrationListRequest(CurrentUserId()));
            return Ok(migrations);
        }

        [HttpGet("{migrationId}")]
        [ProducesResponseType(typeof(MigrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetMigration(string migrationId)
        {
            Logger.LogInformation("Request to get details for migration ID: {MigrationId}", migrationId);
            var migration = await Mediator.Send(new MigrationDetailsRequest(migrationId, CurrentUserId()));
            Logger.LogInformation("Successfully retrieved details for migration ID: {MigrationId}", migrationId);
            return Ok(migration);
        }        


        [HttpGet("{migrationId}/file-content")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrDownloadFile(string migrationId, [FromQuery] string path,
            [FromHeader(Name = "response-content-type")] string? dispositionType)
        {
            bool download = string.Equals(dispositionType, "attachment", StringComparison.OrdinalIgnoreCase);
            var action = download ? "download" : "get";
            Logger.LogInformation("Request to {Action} file {FileName} for migration Migration: {migrationId}, User: {UserId}",
                action, path, migrationId, CurrentUserId());

            var fileRequest = CreateFileRequest(migrationId, path, download);
            var fileResponse = await Mediator.Send(fileRequest);

            if (fileResponse == null)
            {
                return NotFound();
            }

            if (download)
            {
                Logger.LogInformation("Serving file '{FileName}' for download", fileResponse.Filename);
                return File(fileResponse.Content, fileResponse.ContentType, fileResponse.Filename);
            }

            Logger.LogInformation("Returning Base64 content of file '{FileName}' for inline view", fileResponse.Filename);
            return File(fileResponse.Content, fileResponse.ContentType);
        }


        /// <summary>
        /// Downloads all generated files for a migration as a ZIP archive
        /// </summary>
        /// <param name="migrationId">The migration ID</param>
        /// <returns>ZIP file containing all generated files</returns>
        [HttpGet("{migrationId}/files")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DownloadAllFiles(string migrationId)
        {
            Logger.LogInformation("Request to download all files for migration {MigrationId}, User: {UserId}",
                migrationId, CurrentUserId());

            var downloadAllRequest = CreateDownloadAllFilesRequest(migrationId);
            var zipFileResponse = await Mediator.Send(downloadAllRequest);

            if (zipFileResponse == null || zipFileResponse.Content == null || zipFileResponse.Content.Length == 0)
            {
                Logger.LogWarning("No files found for migration {MigrationId}", migrationId);
                return NotFound("No files found for this migration");
            }

            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
            var fileName = $"migration-{migrationId}-files-{timestamp}.zip";

            Logger.LogInformation("Serving ZIP file '{FileName}' with {FileCount} files for download",
                fileName, zipFileResponse.FileCount);

            return File(zipFileResponse.Content, "application/zip", fileName);
        }

        [HttpPost()]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(MigrationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Convert([FromForm] ConvertRequest convertRequest)
        {
            var migrateRequest = CreateMigrateRequest(convertRequest);
            var migration = await Mediator.Send(migrateRequest);

            Logger.LogInformation("Migration initiated successfully. Migration ID: {MigrationId}", Guid.NewGuid());
            return Ok(migration);
        }

        #region Private Methods

        private MigrateRequest CreateMigrateRequest(ConvertRequest convertRequest)
        {
            return _mapper.Map<MigrateRequest>(convertRequest, opt =>
            {
                opt.Items["UserId"] = CurrentUserId();
            });
        }


        private FileRequest CreateFileRequest(string migrationId, string filename, bool download)
        {

            return new FileRequest
            {
                MigrationId = migrationId,
                Filename = filename,
                Download = download,
                UserId = CurrentUserId()
            };
        }

        private DownloadAllFilesRequest CreateDownloadAllFilesRequest(string migrationId)
        {

            return new DownloadAllFilesRequest
            {
                MigrationId = migrationId,
                UserId = CurrentUserId()
            };
        }                

        #endregion
    }
}
