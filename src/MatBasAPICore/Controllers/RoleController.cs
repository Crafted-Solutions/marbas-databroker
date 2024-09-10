using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.Access;
using MarBasAPICore.Routing;
using MarBasCommon;
using MarBasSchema.Access;
using MarBasSchema.Broker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using ISchemaRoleResult = IMarBasResult<ISchemaRole>;
    using ISchemaRolesResult = IMarBasResult<IEnumerable<ISchemaRole>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Role)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class RoleController : ControllerBase
    {
        private readonly ILogger _logger;

        public RoleController(ILogger<RoleController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{id}", Name = "GetRole")]
        [ProducesResponseType(typeof(ISchemaRoleResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaRoleResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, [FromRoute] Guid id)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetRoleAsync(id, cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateRole")]
        [ProducesResponseType(typeof(ISchemaRoleResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaRoleResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, RoleCreateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.CreateRoleAsync(model.Name, model.Entitlement ?? RoleEntitlement.None, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpDelete("{id}", Name = "DeleteRole")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Delete(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, Guid id)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.DeleteRolesAsync(new[] { (Identifiable)id }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreRole")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, RoleUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.StoreRolesAsync(new[] { model.Role }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpGet("Current", Name = "GetCurrentRoles")]
        [ProducesResponseType(typeof(ISchemaRolesResult), StatusCodes.Status200OK)]
        public async Task<ISchemaRolesResult> GetCurrent(CancellationToken cancellationToken, [FromServices] IAsyncAccessService broker)
        {
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.GetContextRolesAsync(cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpGet("List", Name = "ListRoles")]
        [ProducesResponseType(typeof(ISchemaRolesResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISchemaRolesResult> List(CancellationToken cancellationToken, [FromServices] IAsyncSchemaAccessBroker broker, [FromQuery] RoleQueryParametersModel? queryParameters = null)
        {
            HttpResponseException.Throw503IfOffline(broker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await broker.ListRolesAsync(queryParameters?.SortOptions, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }
    }
}
