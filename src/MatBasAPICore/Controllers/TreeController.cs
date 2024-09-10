using System.Globalization;
using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.Grain;
using MarBasAPICore.Routing;
using MarBasSchema.Broker;
using MarBasSchema.Grain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using IGrainsLocalizedResult = IMarBasResult<IEnumerable<IGrainLocalized>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Tree)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class TreeController : ControllerBase
    {
        private readonly ILogger _logger;

        public TreeController(ILogger<TreeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Retrieves Grains by path
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="schemaBroker"></param>
        /// <param name="path">Use * for all Grains under root, Content - for single Grain 'Content', Content/* - for all children of 'Content', Content/** - for all descendants of 'Content' recursively</param>
        /// <param name="lang">ISO language code for string translations (if available)</param>
        /// <param name="queryParameters"></param>
        /// <returns></returns>
        [HttpGet("{*path}", Name = "ResolveGrainPath")]
        [ProducesResponseType(typeof(IEnumerable<IGrainLocalized>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] string? path, [FromQuery] string? lang = null, [FromQuery] GrainQueryParametersModel? queryParameters = null)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.ResolvePathAsync(null == path ? path : Uri.UnescapeDataString(path),
                    string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang),
                    queryParameters?.SortOptions, queryParameters?.ToQueryFilter(), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }
    }
}
