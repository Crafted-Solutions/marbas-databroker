using System.Globalization;
using System.Net.Mime;
using CraftedSolutions.MarBasAPICore;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.GrainTier;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.GrainTier;
using CraftedSolutions.MarBasSchema.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using IGrainFileResult = IMarBasResult<IGrainFile>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.File)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class FileController : ControllerBase
    {
        private readonly ILogger _logger;

        public FileController(ILogger<FileController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetGrainFile")]
        [ProducesResponseType(typeof(IGrainFileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("FileDownload")]
        public async Task<IGrainFileResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang = null, [FromQuery] bool loadContent = false)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetGrainFileAsync(id, loadContent ? GrainFileContentAccess.OnDemand : GrainFileContentAccess.None, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/{disposition:DownloadDisposition}", Name = "DownloadGrainFile")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, MediaTypeNames.Application.Octet)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ResponseCache(Duration = 120)]
        [RequestTimeout("FileDownload")]
        public async Task<IActionResult> Download(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromRoute] DownloadDisposition disposition = DownloadDisposition.Inline, [FromQuery] string? lang = null)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var file = await schemaBroker.GetGrainFileAsync(id, GrainFileContentAccess.OnDemand, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == file)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                var c = file.Content;
                if (null == c)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                Response.RegisterForDispose(c);
                var dispHeader = new ContentDispositionHeaderValue(DownloadDisposition.Attachment == disposition ? "attachment" : "inline")
                {
                    FileName = file.Name
                };
                Response.Headers.ContentDisposition = dispHeader.ToString();
                var etag = (Request.HttpContext.User?.Identity?.Name ?? SchemaDefaults.AnonymousUserName).GetHashCode() ^ file.MTime.ToFileTime() ^ file.Size;
                return File(c is IAsyncStreamableContent casync ? await casync.GetStreamAsync(cancellationToken) : c.Stream, file.MimeType, file.MTime, new EntityTagHeaderValue($"\"{Convert.ToString(etag, 16)}\""), false);
            }, _logger);
        }

        [HttpPut(Name = "UploadNewGrainFile")]
        [ProducesResponseType(typeof(IGrainFileResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("FileUpload")]
        public async Task<IGrainFileResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromForm] GrainFileCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.CreateGrainFileAsync(model.Name,
                    model.File.ContentType ?? MediaTypeNames.Application.Octet, model.File.OpenReadStream(), (Identifiable?)model.ParentId, model.File.Length, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost("{id}", Name = "UploadGrainFile")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("FileUpload")]
        public async Task<CountResult> Post(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromForm] GrainFileUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.StoreGrainFilesAsync(new[] { model.GetGrain(id) }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }
    }
}
