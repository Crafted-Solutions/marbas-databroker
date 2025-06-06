﻿using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CraftedSolutions.MarBasAPICore.Http
{
    public class DevelBasicAuthHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        protected const string ConfigScopeMapRoles = "Auth:MapRoles:";
        protected const string ConfigScopePrincipals = "Auth:Principals:";

        private static bool _isWarned = false;

        protected readonly IConfiguration _configuration;

        public DevelBasicAuthHandler(IConfiguration configuration, IHostEnvironment environment, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _configuration = configuration;
            if (!_isWarned && environment.IsProduction() && Logger.IsEnabled(LogLevel.Warning))
            {
                Logger.LogWarning("Basic authentication is insecure, consider using different method in production");
                _isWarned = true;
            }
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (authorizationHeader != null && authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader["Basic ".Length..].Trim();
                var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                var credentials = credentialsAsEncodedString.Split(':');
                if (!string.IsNullOrEmpty(credentials[0]) && 2 < credentials[0].Length && VerifyCredentials(credentials[0], credentials[1]))
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.Name, credentials[0]),
                        new Claim(ClaimTypes.NameIdentifier, credentials[0]),
                        new Claim(ClaimTypes.Role, MapUserRole(credentials[0]))
                    };
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug("Authenticated {user} as {role}", claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), claimsPrincipal.FindFirstValue(ClaimTypes.Role));
                    }
                    return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }
            }
            Response.StatusCode = 401;
            Response.Headers.WWWAuthenticate = "Basic realm=\"marbas.localhost\"";
            return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization"));
        }

        protected string MapUserRole(string user)
        {
            return _configuration.GetValue($"{ConfigScopeMapRoles}{user}", _configuration.GetValue($"{ConfigScopeMapRoles}*", "Everyone"))!;
        }

        protected bool VerifyCredentials(string user, string password)
        {
            string pwHash = _configuration.GetValue($"{ConfigScopePrincipals}{user}", _configuration.GetValue($"{ConfigScopePrincipals}*", string.Empty));
            if (!string.IsNullOrEmpty(pwHash))
            {
                var msg = Encoding.UTF8.GetBytes(password);
                return SHA512.HashData(msg).AsSpan().SequenceEqual(Convert.FromHexString(pwHash).AsSpan());
            }
            return false;
        }
    }
}
