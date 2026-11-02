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
using System.Globalization;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using IGrainBaseResult = IMarBasResult<IGrainBase>;
    using IGrainPropDefsLocalizedResult = IMarBasResult<IEnumerable<IGrainPropDefLocalized>>;
    using IGrainTypeDefResult = IMarBasResult<IGrainTypeDef>;
    using IGrainTypeLocalizedDefResult = IMarBasResult<IGrainTypeDefLocalized>;
    using StringResult = IMarBasResult<string?>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.TypeDef)]
    [ApiController]
    public sealed class TypeDefController(ILogger<TypeDefController> logger) : ControllerBase
    {
        private readonly ILogger _logger = logger;

        [HttpGet("{id}", Name = "GetTypeDef")]
        [ProducesResponseType(typeof(IGrainTypeLocalizedDefResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainTypeLocalizedDefResult> Get([FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang, CancellationToken cancellationToken = default)
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
        public async Task<IGrainTypeDefResult> Put([FromServices] IAsyncSchemaBroker schemaBroker, TypeDefCreateModel model, CancellationToken cancellationToken = default)
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
        public async Task<CountResult> Store([FromServices] IAsyncSchemaBroker schemaBroker, TypeDefUpdateModel model, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.StoreTypeDefsAsync([model.Grain], cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

        /// <summary>
        /// Convenince API, always returns tier name of the TypeDef itself.
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("Tier", Name = "GetTypeDefNullTier")]
        [ProducesResponseType(typeof(StringResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<StringResult> GetNullTier([FromServices] IAsyncSchemaBroker broker, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetTypeDefTierAsync(null, cancellationToken);
                return MarbasResultFactory.Create<string?>(true, result?.Name);
            }, _logger);
        }

        /// <summary>
        /// Retrieves extension tier name of the TypeDef (like "IFile", "IPropDef", "ITypeDef"),
        /// null is returned for ordinary TypeDefs without tier extension.
        /// </summary>
        /// <param name="broker"></param>
        /// <param name="id">ID of the TypeDef (use "00000000-0000-0000-0000-000000000000" for TypDef itself)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpGet("{id}/Tier", Name = "GetTypeDefTier")]
        [ProducesResponseType(typeof(StringResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<StringResult> GetTier([FromServices] IAsyncSchemaBroker broker, Guid id, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetTypeDefTierAsync((Identifiable)id, cancellationToken);
                return MarbasResultFactory.Create<string?>(true, result?.Name);
            }, _logger);
        }

        /// <summary>
        /// Lists all PropDefs associated with the specified TypeDef and all its mix-ins.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="schemaBroker"></param>
        /// <param name="id">ID of the TypeDef</param>
        /// <param name="lang">Language code to use for response localization</param>
        /// <returns></returns>
        [HttpGet("{id}/Properties", Name = "GetProperties")]
        [ProducesResponseType(typeof(IGrainPropDefsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainPropDefsLocalizedResult> GetProperties([FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, [FromQuery] string? lang, CancellationToken cancellationToken = default)
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
        public async Task<IGrainBaseResult> GetOrCreateDefaults([FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] Guid id, CancellationToken cancellationToken = default)
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
