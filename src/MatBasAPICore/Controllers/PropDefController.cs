using System.Globalization;
using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.GrainDef;
using MarBasAPICore.Routing;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Broker;
using MarBasSchema.GrainDef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using IGrainPropDefLocalizedResult = IMarBasResult<IGrainPropDefLocalized>;
    using IGrainPropDefResult = IMarBasResult<IGrainPropDef>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.PropDef)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class PropDefController : ControllerBase
    {
        private readonly ILogger _logger;

        public PropDefController(ILogger<PropDefController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetPropDef")]
        [ProducesResponseType(typeof(IGrainPropDefLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainPropDefLocalizedResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetPropDefAsync(id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPut(Name = "CreatePropDef")]
        [ProducesResponseType(typeof(IGrainPropDefResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainPropDefResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, PropDefCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.CreatePropDefAsync(model.Name, (Identifiable)model.TypeContainerId,
                    string.IsNullOrEmpty(model.ValueType) ? TraitValueType.Text : Enum.Parse<TraitValueType>(model.ValueType, true), model.CardinalityMin ?? 1, model.CardinalityMax ?? 1, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }


        [HttpPost(Name = "StorePropDef")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, PropDefUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.StoreGrainPropDefsAsync(new[] { model.Grain }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }
    }
}
