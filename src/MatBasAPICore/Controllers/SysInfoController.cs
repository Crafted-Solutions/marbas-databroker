using MarBasAPICore.Routing;
using MarBasSchema.Broker;
using MarBasSchema.Sys;
using Microsoft.AspNetCore.Mvc;

namespace MarBasAPICore.Controllers
{
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.SysInfo)]
    [ApiController]
    public sealed class SysInfoController : ControllerBase
    {
        private readonly IServerInfo _info = new ServerInfo();

        public SysInfoController([FromServices] IBrokerProfile profile)
        {
            _info.SchemaVersion = profile.Version;
            _info.InstanceId = profile.InstanceId;
        }

        [HttpGet(Name = "GetSysInfo")]
        public IServerInfo Get()
        {
            return _info;
        }
    }
}
