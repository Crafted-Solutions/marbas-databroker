using System.Globalization;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.Grain;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Grain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using FlagResult = IMarBasResult<bool>;
    using GrainTraitsMapResult = IMarBasResult<GrainTraitsMap>;
    using IGrainBaseResult = IMarBasResult<IGrainBase>;
    using IGrainFlagMapResult = IMarBasResult<IDictionary<Guid, bool>>;
    using IGrainLocalizedResult = IMarBasResult<IGrainLocalized>;
    using IGrainsLocalizedResult = IMarBasResult<IEnumerable<IGrainLocalized>>;
    using IGrainLabelsResult = IMarBasResult<IEnumerable<IGrainLabel>>;
    using ISchemaAclsResult = IMarBasResult<IEnumerable<ISchemaAclEntry>>;
    using StringResult = IMarBasResult<string?>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Grain)]
    [ApiController]
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
        public async Task<IGrainLocalizedResult> Get([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] string? lang, CancellationToken cancellationToken = default)
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
        public async Task<IGrainBaseResult> Put([FromServices] IAsyncSchemaBroker broker, GrainCreateModel model, CancellationToken cancellationToken = default)
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
        public async Task<CountResult> Delete([FromServices] IAsyncSchemaBroker broker, Guid id, CancellationToken cancellationToken = default)
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
        public async Task<CountResult> Store([FromServices] IAsyncSchemaBroker broker, GrainUpdateModel model, CancellationToken cancellationToken = default)
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
        public async Task<IGrainBaseResult> CloneGrain([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, GrainCloneModel model, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.CloneGrainAsync((Identifiable)id, (Identifiable?)model.NewParentId, model.Depth ?? GrainCloneDepth.Self, model.CopyACL ?? false, cancellationToken);
                return MarbasResultFactory.Create(null != result, result);
            }, _logger);
        }

        [HttpPost("{id}/Move", Name = "MoveGrain")]
        [ProducesResponseType(typeof(IGrainBaseResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainBaseResult> MoveGrain([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, Guid newParentId, CancellationToken cancellationToken = default)
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
        public async Task<FlagResult> IsInstanceOf([FromServices] IAsyncSchemaBroker broker, Guid id, Guid typeDefId, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.IsGrainInstanceOfAsync((Identifiable)id, (Identifiable)typeDefId, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/Tier", Name = "GetGrainTier")]
        [ProducesResponseType(typeof(StringResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<StringResult> GetGrainTier([FromServices] IAsyncSchemaBroker broker, Guid id, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetGrainTierAsync((Identifiable)id, cancellationToken);
                return MarbasResultFactory.Create<string?>(true, result?.Name);
            }, _logger);
        }

        [HttpGet("List", Name = "ListRootGrains")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> ListRoot([FromServices] IAsyncSchemaBroker broker, [FromQuery] bool recursive = false,
            [FromQuery] string? lang = null, [FromQuery] GrainQueryParametersModel? queryParameters = null, CancellationToken cancellationToken = default)
        {
            return await List(broker, null, recursive, lang, queryParameters, cancellationToken);
        }

        [HttpGet("{id}/List", Name = "ListGrains")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> List([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid? id = null, [FromQuery] bool recursive = false,
            [FromQuery] string? lang = null, [FromQuery] GrainQueryParametersModel? queryParameters = null, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.ListGrainsAsync((Identifiable?)id, recursive, string.IsNullOrEmpty(lang) ? null : CultureInfo.GetCultureInfo(lang),
                    queryParameters?.SortOptions, queryParameters?.ToQueryFilter(), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost("VerifyExist", Name = "VerifyGrainsExist")]
        [ProducesResponseType(typeof(IGrainFlagMapResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainFlagMapResult> VerifyGrainsExist([FromServices] IAsyncSchemaBroker broker, IEnumerable<Guid> grainIdsToCheck, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.VerifyGrainsExistAsync(grainIdsToCheck, cancellationToken);
                return MarbasResultFactory.Create(result.Any(), result);
            }, _logger);
        }

        [HttpGet("{id}/Labels", Name = "GetGrainLabels")]
        [ProducesResponseType(typeof(IGrainLabelsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainLabelsResult> Labels([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] IEnumerable<string>? lang = null, CancellationToken cancellationToken = default)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetGrainLabelsAsync(new[] { id }, lang?.Select(x => CultureInfo.GetCultureInfo(x)), cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("{id}/Path", Name = "GetGrainAncestors")]
        [ProducesResponseType(typeof(IGrainsLocalizedResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IGrainsLocalizedResult> Path([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] bool includeSelf = false,
            [FromQuery] string? lang = null, CancellationToken cancellationToken = default)
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
        public async Task<GrainTraitsMapResult> GetTraits([FromServices] IAsyncSchemaBroker broker, [FromRoute] Guid id, [FromQuery] string? lang = null, CancellationToken cancellationToken = default)
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
        public async Task<ISchemaAclsResult> GetAcl([FromServices] IAsyncSchemaAccessBroker broker, [FromRoute] Guid id, CancellationToken cancellationToken = default)
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
