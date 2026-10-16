using CraftedSolutions.MarBasAPICore;
using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Models.Access;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon;
using CraftedSolutions.MarBasSchema.Access;
using CraftedSolutions.MarBasSchema.Broker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using ISchemaAclResult = IMarBasResult<ISchemaAclEntry>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Acl)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class AclController : ControllerBase
    {
        private readonly ILogger _logger;

        public AclController(ILogger<AclController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{roleId}/{grainId}", Name = "GetAclEntry")]
        [ProducesResponseType(typeof(ISchemaAclResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaAclResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, [FromRoute] Guid roleId, [FromRoute] Guid grainId)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetAclEntryAsync((Identifiable)roleId, (Identifiable)grainId, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }


        [HttpDelete("{roleId}/{grainId}", Name = "DeleteAclEntry")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Delete(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, [FromRoute] Guid roleId, [FromRoute] Guid grainId)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.DeleteAclAsync(new[] { new SchemaAclEntry((Identifiable)roleId, (Identifiable)grainId) }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateAclEntry")]
        [ProducesResponseType(typeof(ISchemaAclResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaAclResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, AclEntryCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.CreateAclEntryAsync((Identifiable)model.RoleId,
                    (Identifiable)model.GrainId, model.PermissionMask ?? GrainAccessFlag.Read,
                    model.RestrictionMask ?? GrainAccessFlag.None, model.Inherit ?? true, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreAclEntry")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, AclEntryUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.StoreAclAsync(new[] { model.Entry }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

    }
}
