using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.Transport;
using MarBasAPICore.Routing;
using MarBasCommon;
using MarBasSchema.Broker;
using MarBasSchema.Transport;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using ITransportableGrainResponse = IMarBasResult<IEnumerable<IGrainTransportable>>;
    using IGrainImportResultsResponse = IMarBasResult<IGrainImportResults>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Transport)]
    [ApiController]
    public sealed class TransportController: ControllerBase
    {
        private readonly ILogger _logger;

        public TransportController(ILogger<TransportController> logger)
        {
            _logger = logger;
        }

        [HttpPost("Out", Name = "TransportOut")]
        [ProducesResponseType(typeof(ITransportableGrainResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
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
        public async Task<IGrainImportResultsResponse> In(GrainImportModel imports, [FromServices] IAsyncSchemaBroker broker, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.ImportGrainsAsync(imports.Grains, imports.GrainsToDelete?.Select(x => (Identifiable) x), imports.DuplicatesHandling ?? DuplicatesHandlingStrategy.Merge, cancellationToken);
                return MarbasResultFactory.Create(0 < (result.ImportedCount + result.DeletedCount + result.IgnoredCount)
                    && (null == result.Feedback || !result.Feedback.Any(x => x.FeedbackType >= LogLevel.Warning)), result);
            }, _logger);
        }
    }
}
