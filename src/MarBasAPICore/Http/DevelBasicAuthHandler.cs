using CraftedSolutions.MarBasAPICore.Auth;
using CraftedSolutions.MarBasSchema;
using CraftedSolutions.MarBasSchema.Access;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;

namespace CraftedSolutions.MarBasAPICore.Http
{
    public class DevelBasicAuthHandler: AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private static bool _isWarned = false;

        protected readonly BasicAuthConfigBackend? _authConfig;

        public DevelBasicAuthHandler(IConfiguration configuration, IHostEnvironment environment, IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
            : base(options, logger, encoder)
        {
            _authConfig = AuthConfig.Bind(configuration.GetSection(configuration.GetValue(AuthConfig.SectionSwitch, AuthConfig.SectionName)), true) as BasicAuthConfigBackend;
            if (Logger.IsEnabled(LogLevel.Warning))
            {
                if (null == _authConfig)
                {
                    Logger.LogWarning("Configuration for basic authentication is missing");
                }
                else if (!_isWarned && environment.IsProduction())
                {
                    Logger.LogWarning("Basic authentication is insecure, consider using different method in production");
                    _isWarned = true;
                }
            }
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return await Task.FromResult(AuthenticateResult.NoResult());
            }

            var schema = _authConfig?.Schema ?? "Basic";
            var authorizationHeader = Request.Headers.Authorization.ToString();
            if (authorizationHeader != null && authorizationHeader.StartsWith(schema, StringComparison.OrdinalIgnoreCase))
            {
                var token = authorizationHeader[$"{schema} ".Length..].Trim();
                var (user, pw) = ParseCredentials(token);
                if (!string.IsNullOrEmpty(user) && 0 < pw.Length && VerifyCredentials(user, pw))
                {
                    var idClaim = $"{user}{SchemaDefaults.InternalPrincipalSuffix}";
                    var claims = new[] {
                        new Claim(ClaimTypes.Name, user),
                        new Claim(SchemaDefaults.UserIdentifierClaimType, idClaim),
                        new Claim(ClaimTypes.NameIdentifier, idClaim),
                        new Claim(ClaimTypes.Role, MapUserRole(user))
                    };
                    var identity = new ClaimsIdentity(claims, schema, SchemaDefaults.UserIdentifierClaimType, null);
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    if (Logger.IsEnabled(LogLevel.Debug))
                    {
                        Logger.LogDebug("Authenticated {user} as {role}", claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), claimsPrincipal.FindFirstValue(ClaimTypes.Role));
                    }
                    return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
                }
            }
            Response.StatusCode = 401;
            Response.Headers.WWWAuthenticate = $"{schema} realm=\"marbas.localhost\"";
            return await Task.FromResult(AuthenticateResult.Fail("Invalid Authorization"));
        }

        protected string MapUserRole(string user)
        {
            if ((true != _authConfig?.MapRoles.TryGetValue(user, out var result) && true != _authConfig?.MapRoles.TryGetValue("*", out result)) || string.IsNullOrEmpty(result))
            {
                result = SchemaRole.Everyone.Name;
            }
            return result;
        }

        protected bool VerifyCredentials(string user, byte[] password)
        {
            var pwHash = _authConfig?.GetPasswordHash(user);
            return null != pwHash && SHA512.HashData(password).AsSpan()
                    .SequenceEqual(pwHash.AsSpan());
        }

        protected static (string User, byte[] Pw) ParseCredentials(string token)
        {
            var user = new StringBuilder();
            var pw = Array.Empty<byte>();

            var sepFound = false;
            using (var bin = new MemoryStream(Convert.FromBase64String(token)))
            using (var reader = new BinaryReader(bin, Encoding.UTF8))
            using (var bout = new MemoryStream())
            using(var writer = new BinaryWriter(bout, Encoding.UTF8))
            {
                while (-1 != reader.PeekChar())
                {
                    var c = reader.ReadChar();
                    if (!sepFound && ':' == c)
                    {
                        sepFound = true;
                        continue;
                    }
                    if (sepFound)
                    {
                        writer.Write(c);
                    }
                    else
                    {
                        user.Append(c);
                    }
                }
                writer.Flush();
                pw = bout.ToArray();
            }
            return (user.ToString(), pw);
        }
    }
}
