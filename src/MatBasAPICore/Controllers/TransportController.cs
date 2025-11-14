using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.Transport;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasCommon.Job;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Transport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using IBackgroundJobResponse = IMarBasResult<IBackgroundJob?>;
    using IGrainImportResultsResponse = IMarBasResult<IGrainImportResults>;
    using ITransportableGrainResponse = IMarBasResult<IEnumerable<IGrainTransportable>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Transport)]
    [ApiController]
    public sealed class TransportController(ILogger<TransportController> logger) : ControllerBase
    {
        private readonly ILogger _logger = logger;

        [HttpPost("Out", Name = "TransportOut")]
        [ProducesResponseType(typeof(ITransportableGrainResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("Export")]
        public async Task<ITransportableGrainResponse> Out(IEnumerable<Guid> grainIds, [FromServices] IAsyncSchemaBroker schemaBroker, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.ExportGrainsAsync(grainIds.Select(x => (Identifiable)x), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPut("In", Name = "TransportIn")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(IGrainImportResultsResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("Import")]
        public async Task<IGrainImportResultsResponse> In(GrainImportModel imports, [FromServices] IAsyncSchemaBroker broker, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.ImportGrainsAsync(imports.Grains, imports.GrainsToDelete?.Select(x => (Identifiable)x), imports.DuplicatesHandling ?? DuplicatesHandlingStrategy.Merge, cancellationToken);
                return MarbasResultFactory.Create(0 < result.ImportedCount + result.DeletedCount + result.IgnoredCount
                    && (null == result.Feedback || !result.Feedback.Any(x => x.FeedbackType >= LogLevel.Warning)), result);
            }, _logger);
        }

        [HttpPost("PackageOut", Name = "PackageOut")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK, MediaTypeNames.Application.Zip)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ResponseCache(Duration = 0, NoStore = true)]
        [RequestTimeout("Export")]
        public async Task<IActionResult> PackageOut(PackageExportModel exportRequest, [FromServices] IAsyncSchemaPackager schemaPackager, [FromServices] IBrokerProfile brokerProfile, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(brokerProfile);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var pkgStream = await schemaPackager.ExportPackageAsync(exportRequest.Items, cancellationToken);
                Response.RegisterForDispose(pkgStream);


                var packageNamePfx = exportRequest.NamePrefix ?? SchemaPackager.PackagePrefix;
                var ts = DateTime.UtcNow;
                var etag = (Request.HttpContext.User?.Identity?.Name ?? SchemaDefaults.AnonymousUserName).GetHashCode() ^ ts.Ticks ^ pkgStream.Length;

                return File(pkgStream, MediaTypeNames.Application.Zip
                    , $"{packageNamePfx}{brokerProfile.InstanceId:D}-{ts:yyyyMMddHHmmssfff}.zip"
                    , ts, new EntityTagHeaderValue($"\"{Convert.ToString(etag, 16)}\""), false);
            }, _logger);
        }

        [HttpPut("PackageIn", Name = "PackageIn")]
        [DisableRequestSizeLimit]
        [ProducesResponseType(typeof(IBackgroundJobResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [RequestTimeout("Import")]
        [RequestSizeLimit(0x40000000), RequestFormLimits(MultipartBodyLengthLimit = 0x40000000, ValueLengthLimit = 0x40000000)] // 1 GiB
        public async Task<IBackgroundJobResponse> PackageIn([FromForm] PackageImportModel importRequest, [FromServices] IAsyncSchemaPackager schemaPackager, [FromServices] IBrokerProfile brokerProfile, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(brokerProfile);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return MarbasResultFactory.Create<IBackgroundJob?>(false, null);
                }

                var result = await schemaPackager.SchedulePackageImportAsync(importRequest.Content.OpenReadStream()
                    , importRequest.DuplicatesHandling ?? DuplicatesHandlingStrategy.MergeSkipNewer, importRequest.MissingDependencyHandling ?? MissingDependencyHandlingStrategy.CreatePlaceholder, cancellationToken);
                return MarbasResultFactory.Create<IBackgroundJob?>(true, result);
            }, _logger);
        }
    }
}
