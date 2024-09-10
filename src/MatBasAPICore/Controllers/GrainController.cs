using System.Globalization;
using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.Grain;
using MarBasAPICore.Routing;
using MarBasCommon;
using MarBasSchema;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using MarBasSchema.Grain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using FlagResult = IMarBasResult<bool>;
    using GrainTraitsMapResult = IMarBasResult<GrainTraitsMap>;
    using IGrainBaseResult = IMarBasResult<IGrainBase>;
    using IGrainLocalizedResult = IMarBasResult<IGrainLocalized>;
    using IGrainsLocalizedResult = IMarBasResult<IEnumerable<IGrainLocalized>>;
    using ISchemaAclsResult = IMarBasResult<IEnumerable<ISchemaAclEntry>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Grain)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class GrainController : ControllerBase
    {
        private readonly ILogger _logger;

        public GrainController(ILogger<GrainController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetGrain")]
        [ProducesResponseType(typeof(IGrainLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainLocalizedResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] string? lang)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = SchemaDefaults.AnyGrainID == id ? null : await broker.GetGrainAsync(id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateGrain")]
        [ProducesResponseType(typeof(IGrainBaseResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainBaseResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, GrainCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                IIdentifiable? typeId = null;
                if (null == model.TypeDefId)
                {
                    typeId = (Identifiable)SchemaDefaults.ElementTypeDefID;
                }
                else if (!SchemaDefaults.TypeDefTypeDefID.Equals(model.TypeDefId))
                {
                    typeId = (Identifiable)model.TypeDefId;
                }
                var result = await broker.CreateGrainAsync(model.Name, (Identifiable)model.ParentId, typeId, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpDelete("{id}", Name = "DeleteGrain")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Delete(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, Guid id)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.DeleteGrainsAsync(new[] { (Identifiable)id }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreGrain")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, GrainUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.StoreGrainsAsync(new[] { model.Grain }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

        [HttpPost("{id}/Clone", Name = "CloneGrain")]
        [ProducesResponseType(typeof(IGrainBaseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainBaseResult> CloneGrain(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, GrainCloneModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.CloneGrainAsync((Identifiable)id, (Identifiable?) model.NewParentId, model.Depth ?? GrainCloneDepth.Self, model.CopyACL ?? false, cancellationToken);
                return MarbasResultFactory.Create(null != result, result);
            }, _logger);
        }

        [HttpPost("{id}/Move", Name = "MoveGrain")]
        [ProducesResponseType(typeof(IGrainBaseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainBaseResult> MoveGrain(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, Guid newParentId)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.MoveGrainAsync((Identifiable)id, (Identifiable)newParentId, cancellationToken);
                return MarbasResultFactory.Create(null != result, result);
            }, _logger);
        }

        [HttpGet("{id}/InstanceOf/{typeDefId}", Name = "IsGrainInstanceOf")]
        [ProducesResponseType(typeof(FlagResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<FlagResult> IsInstanceOf(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, Guid id, Guid typeDefId)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.IsGrainInstanceOfAsync((Identifiable)id, (Identifiable)typeDefId, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("List", Name = "ListRootGrains")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> ListRoot(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromQuery] bool recursive = false, [FromQuery] string? lang = null, [FromQuery] GrainQueryParametersModel? queryParameters = null)
        {
            return await List(cancellationToken, broker, null, recursive, lang, queryParameters);
        }

        [HttpGet("{id}/List", Name = "ListGrains")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> List(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid? id = null, [FromQuery] bool recursive = false, [FromQuery] string? lang = null, [FromQuery] GrainQueryParametersModel? queryParameters = null)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.ListGrainsAsync((Identifiable?)id, recursive, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang),
                    queryParameters?.SortOptions, queryParameters?.ToQueryFilter(), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/Path", Name = "GetGrainAncestors")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> Path(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] bool includeSelf = false, [FromQuery] string? lang = null)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetGrainAncestorsAsync((Identifiable)id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), includeSelf, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/Traits", Name = "GetGrainTraits")]
        [ProducesResponseType(typeof(GrainTraitsMapResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<GrainTraitsMapResult> GetTraits(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] string? lang = null)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetGrainTraitsAsync((Identifiable)id, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/Acl", Name = "GetGrainAcl")]
        [ProducesResponseType(typeof(ISchemaAclsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaAclsResult> GetAcl(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, [FromRoute] Guid id)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetEffectiveAclAsync((Identifiable)id, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }
    }
}
