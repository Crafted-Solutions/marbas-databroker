using System.Globalization;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.Trait;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using ITraitResult = IMarBasResult<ITraitBase>;
    using ITraitsResult = IMarBasResult<IEnumerable<ITraitBase>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Trait)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class TraitController : ControllerBase
    {
        private readonly ILogger _logger;

        public TraitController(ILogger<TraitController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetTrait")]
        [ProducesResponseType(typeof(ITraitResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ITraitResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetTraitAsync((Identifiable)id, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpDelete("{id}", Name = "DeleteTrait")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Delete(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, Guid id)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.DeleteTraitsAsync(new[] { (Identifiable)id }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreTrait")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, TraitUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.StoreTraitsAsync(new[] { model.Trait }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateTrait")]
        [ProducesResponseType(typeof(ITraitResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ITraitResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, TraitCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.CreateTraitAsync(model.Ref, model.Value, model.Ord ?? 0, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("Values/{grainId}/{propdefId}", Name = "GetTraitValues")]
        [ProducesResponseType(typeof(ITraitsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ITraitsResult> GetTraitValues(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker,
            [FromRoute] Guid grainId, [FromRoute] Guid propdefId, [FromQuery] int revision = 1, [FromQuery] string? lang = null)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var traitRef = new TraitRef((Identifiable)grainId, (Identifiable)propdefId, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang)) { Revision = revision };
                var result = await schemaBroker.GetTraitValuesAsync(traitRef, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost("Values", Name = "SetTraitValues")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> SetTraitValues(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, TraitValuesReplaceModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var traitRef = new TraitRef((Identifiable)model.GrainId, null == model.ValueType ? (Identifiable)model.PropDefId : new SimpleValueTypeContraint((Identifiable)model.PropDefId, (TraitValueType)model.ValueType),
                    string.IsNullOrEmpty(model.Culture) ? null : CultureInfo.GetCultureInfo(model.Culture))
                { Revision = model.Revision };
                var result = await schemaBroker.ReplaceTraitValuesAsync(traitRef, model.Values, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpDelete("Values/{grainId}/{propdefId}", Name = "DeleteTraitValues")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> DeleteTraitValues(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker,
            [FromRoute] Guid grainId, [FromRoute] Guid propdefId, [FromQuery] int revision = 1, [FromQuery] string? lang = null)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var traitRef = new TraitRef((Identifiable)grainId, (Identifiable)propdefId, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang)) { Revision = revision };
                var result = await schemaBroker.ResetTraitValuesAsync(traitRef, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }
    }
}
