using CraftedSolutions.MarBasAPICore.Http;
using CraftedSolutions.MarBasAPICore.Models;
using CraftedSolutions.MarBasAPICore.Routing;
using CraftedSolutions.MarBasCommon.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    using IBackgroundJobResult = IMarBasResult<IBackgroundJob>;
    using IBackgroundJobsResult = IMarBasResult<IEnumerable<IBackgroundJob>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.BackgroundJob)]
    [ApiController]
    public sealed class BackgroundJobController(ILogger<BackgroundJobController> logger): ControllerBase
    {
        private readonly ILogger<BackgroundJobController> _logger = logger;

        [HttpGet("{id}", Name = "GetJob")]
        [ProducesResponseType(typeof(IBackgroundJobResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IBackgroundJobResult Get([FromServices] IBackgroundJobManager jobManager, [FromRoute] Guid id, [FromQuery] bool autoRemove = false)
        {
            return HttpResponseException.DigestExceptions(() =>
            {
                var result = jobManager.GetJob(id, autoRemove);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpDelete("{id}", Name = "RemoveJob")]
        [ProducesResponseType(typeof(IBackgroundJobResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IBackgroundJobResult Remove([FromServices] IBackgroundJobManager jobManager, [FromRoute] Guid id, [FromQuery] bool cancel = false)
        {
            return HttpResponseException.DigestExceptions(() =>
            {
                var result = jobManager.RemoveJob(id, cancel);
                return MarbasResultFactory.Create(null != result, result);
            }, _logger);
        }

        [HttpGet("List", Name = "ListJobs")]
        [ProducesResponseType(typeof(IBackgroundJobsResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        public IBackgroundJobsResult List([FromServices] IBackgroundJobManager jobManager, [FromQuery] bool forAllUsers = false)
        {
            return HttpResponseException.DigestExceptions(() =>
            {
                var result = jobManager.ListJobs(forAllUsers);
                return MarbasResultFactory.Create(true, result);
            });
        }
    }
}
