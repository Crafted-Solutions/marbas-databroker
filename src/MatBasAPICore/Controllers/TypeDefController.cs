using System.Globalization;
using CraftedSolutions.MarBasAPICore;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.GrainDef;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using CraftedSolutions.MarBasSchema.GrainDef;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using IGrainPropDefsLocalizedResult = IMarBasResult<IEnumerable<IGrainPropDefLocalized>>;
    using IGrainTypeDefResult = IMarBasResult<IGrainTypeDef>;
    using IGrainTypeLocalizedDefResult = IMarBasResult<IGrainTypeDefLocalized>;
    using IGrainBaseResult = IMarBasResult<IGrainBase>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.TypeDef)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class TypeDefController : ControllerBase
    {
        private readonly ILogger _logger;

        public TypeDefController(ILogger<TypeDefController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetTypeDef")]
        [ProducesResponseType(typeof(IGrainTypeLocalizedDefResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainTypeLocalizedDefResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetTypeDefAsync(id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateTypeDef")]
        [ProducesResponseType(typeof(IGrainTypeDefResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainTypeDefResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, TypeDefCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.CreateTypeDefAsync(model.Name, (Identifiable?)model.ParentId, model.Impl, model.MixInIds?.Select((id) => (Identifiable)id), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreTypeDef")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, TypeDefUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.StoreGrainTypeDefsAsync(new[] { model.Grain }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

        /// <summary>
        /// Lists all PropDefs associated with the specified TypeDef and all its mix-ins.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="schemaBroker"></param>
        /// <param name="id"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpGet("{id}/Properties", Name = "GetProperties")]
        [ProducesResponseType(typeof(IGrainPropDefsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainPropDefsLocalizedResult> GetProperties(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetTypeDefPropertiesAsync((Identifiable)id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        /// <summary>
        /// Retrieves default instance of the specified TypeDef, one such instance will be created if none exists yet
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="schemaBroker"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}/Defaults", Name = "GetOrCreateDefaults")]
        [ProducesResponseType(typeof(IGrainBaseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainBaseResult> GetOrCreateDefaults(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetOrCreateTypeDefDefaultsAsync((Identifiable)id, cancellationToken);
                return MarbasResultFactory.Create(null != result, result);
            }, _logger);
        }
    }
}
