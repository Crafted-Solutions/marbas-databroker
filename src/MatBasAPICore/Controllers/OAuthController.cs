using CraftedSolutions.MarBasAPICore.Auth;
using CraftedSolutions.MarBasAPICore.Routing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Mime;

namespace CraftedSolutions.MarBasAPICore.Controllers
{
    [AllowAnonymous]
    [Route($"{RoutingConstants.DefaultPrefix}/[controller]", Order = (int)ControllerPrority.OAuth)]
    [ApiController]
    public sealed class OAuthController(IConfiguration configuration, HttpClient httpClient) : ControllerBase
    {
        private readonly IAuthConfig? _authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true);
        private readonly HttpClient _httpClient = httpClient;

        [HttpPost("Token", Name = "ForwardToken")]
        public async Task<IActionResult> Token(CancellationToken cancellationToken = default)
        {
            if (_authConfig is OIDCAuthConfigBackend config && config.UseTokenProxy)
            {
                var uri = new Uri(config.TokenUrl);

                Request.EnableBuffering();
                using (var requestMessage = new HttpRequestMessage())
                {
                    if (Request.HasFormContentType)
                    {
                        if (Request.ContentType!.StartsWith(MediaTypeNames.Application.FormUrlEncoded))
                        {
                            var formContent = new List<KeyValuePair<string, string>>();
                            foreach (var formKey in Request.Form.Keys)
                            {
                                var content = Request.Form[formKey].FirstOrDefault();
                                if (content != null)
                                    formContent.Add(new KeyValuePair<string, string>(formKey, content));
                            }
                            requestMessage.Content = new FormUrlEncodedContent(formContent);
                        }
                        else
                        {
                            var multipartFormDataContent = new MultipartFormDataContent();
                            foreach (var formKey in Request.Form.Keys)
                            {
                                var content = Request.Form[formKey].FirstOrDefault();
                                if (content != null)
                                    multipartFormDataContent.Add(new StringContent(content), formKey);
                            }
                            requestMessage.Content = multipartFormDataContent;
                        }
                    }
                    else
                    {
                        Request.Body.Seek(0, SeekOrigin.Begin);
                        var streamContent = new StreamContent(Request.Body);
                        requestMessage.Content = streamContent;

                    }
                    foreach (var header in Request.Headers)
                    {
                        if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
                        {
                            requestMessage.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                        }
                    }

                    //if (!isAuthorizeProxied)
                    //{
                    //    requestMessage.Headers.Remove("Authorization");
                    //}

                    requestMessage.Headers.Host = uri.Authority;
                    requestMessage.RequestUri = uri;
                    requestMessage.Method = new HttpMethod(Request.Method);

                    using (var responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken))
                    {
                        Response.StatusCode = (int)responseMessage.StatusCode;

                        foreach (var header in responseMessage.Headers)
                        {
                            Response.Headers[header.Key] = header.Value.ToArray();
                        }

                        foreach (var header in responseMessage.Content.Headers)
                        {
                            Response.Headers[header.Key] = header.Value.ToArray();
                        }

                        Response.Headers.Remove("transfer-encoding");

                        using (var respStream = await responseMessage.Content.ReadAsStreamAsync(cancellationToken))
                        {
                            await respStream.CopyToAsync(Response.Body, 0x14000, cancellationToken);
                        }
                    }
                }
                return new EmptyResult();
            }
            throw new ApplicationException($"Unsupported configuration {_authConfig?.GetType()}");
        }
    }
}
