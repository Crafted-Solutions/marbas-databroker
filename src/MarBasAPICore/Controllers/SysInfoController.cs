using CraftedSolutions.MarBasAPICore.Auth;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasSchema.Broker;
using CraftedSolutions.MarBasSchema.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    [AllowAnonymous]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.SysInfo)]
    [ApiController]
    public sealed class SysInfoController : ControllerBase
    {
        private readonly IServerInfo _info = new ServerInfo();
        private readonly IAuthConfig? _authConfig;

        public SysInfoController([FromServices] IBrokerProfile profile, IConfiguration configuration)
        {
            _info.SchemaVersion = profile.Version;
            _info.InstanceId = profile.InstanceId;
            _authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)));
        }

        [HttpGet(Name = "GetSysInfo")]
        public IServerInfo Get()
        {
            return _info;
        }

        [HttpGet("AuthConfig", Name = "GetAuthConfig")]
        public IAuthConfig? GetAuthConfig()
        {
            return _authConfig;
        }
    }
}
