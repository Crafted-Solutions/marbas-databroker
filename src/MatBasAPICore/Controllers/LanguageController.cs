using System.Globalization;
using MarBasAPICore.Http;
using MarBasAPICore.Models;
using MarBasAPICore.Models.Sys;
using MarBasAPICore.Routing;
using MarBasSchema.Broker;
using MarBasSchema.Sys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MarBasAPICore.Controllers
{
    using CountResult = IMarBasResult<int>;
    using ISystemLanguageResult = IMarBasResult<ISystemLanguage>;
    using ISystemLanguagesResult = IMarBasResult<IEnumerable<ISystemLanguage>>;

    [Authorize]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.Language)]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1068:CancellationToken parameters must come last", Justification = "We want the token without interference with default parameters")]
    public sealed class LanguageController: ControllerBase
    {
        private readonly ILogger _logger;

        public LanguageController(ILogger<LanguageController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{lang}", Name = "GetLanguage")]
        [ProducesResponseType(typeof(ISystemLanguageResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISystemLanguageResult> Get(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] string lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.GetSystemLanguageAsync(CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status404NotFound);
                }
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpDelete("{lang}", Name = "DeleteLanguage")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Delete(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromRoute] string lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.DeleteSystemLanguagesAsync(new[] { (SystemLanguage)CultureInfo.GetCultureInfo(lang) }, cancellationToken);
                return MarbasResultFactory.Create(0 < result, result);
            }, _logger);
        }

        [HttpPut(Name = "CreateLanguage")]
        [ProducesResponseType(typeof(ISystemLanguageResult), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISystemLanguageResult> Put(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, string lang)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                var result = await schemaBroker.CreateSystemLanguageAsync(CultureInfo.GetCultureInfo(lang), cancellationToken);
                if (null == result)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                Response.StatusCode = StatusCodes.Status201Created;
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }

        [HttpPost(Name = "StoreLanguage")]
        [ProducesResponseType(typeof(CountResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<CountResult> Store(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, LanguageUpdateModel model)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                if (model.Language.IsoCode == CultureInfo.InvariantCulture.IetfLanguageTag)
                {
                    throw new HttpResponseException(StatusCodes.Status400BadRequest);
                }
                var result = await schemaBroker.StoreSystemLanguagesAsync(new[] { model.Language }, cancellationToken);
                return MarbasResultFactory.Create(0 != result, result);
            }, _logger);
        }

        [HttpGet("List", Name = "ListLanguages")]
        [ProducesResponseType(typeof(ISystemLanguagesResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ISystemLanguagesResult> List(CancellationToken cancellationToken, [FromServices] IAsyncSchemaBroker schemaBroker, [FromQuery] LanguageQueryParametersModel? queryParameters = null)
        {
            HttpResponseException.Throw503IfOffline(schemaBroker);
            return await HttpResponseException.DigestExceptionsAsync(async () =>
            {
                IEnumerable<CultureInfo>? cultures = null;
                if (null != queryParameters && null != queryParameters.LangFilter)
                {
                    cultures = LanguageQueryParametersModel.GetCultures(queryParameters.LangFilter);
                }
                var result = await schemaBroker.ListSystemLanguagesAsync(cultures, cancellationToken);
                return MarbasResultFactory.Create(true, result);
            }, _logger);
        }
    }
}
