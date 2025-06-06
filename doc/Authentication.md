# Configuring Authentication
MarBas databroker currently supports Basic and OAuth aithentication schemes. Although the basis distribution ships precofigured with Basic authentication for development and production, we strongly recommend to use OAuth in production environment.

For configuration you use either the predefined `Auth` section in `appsettings.json` (not recommended) or a custom dedicated section named by `UseAuth` configuration key, which section you can (and should) extract into a separate file `authsettings.json` or resp. `authsettings.<ASPNETCORE_ENVIRONMENT>.json` since there may be confidential data in there (the `authsettings.json` will be always loaded by the app if present in the same directory as `appsettings.json`). In the current document `Auth` stands as placeholder for any custom section name.

## MarBas Roles
All access rights within MarBas system are linked to roles, these roles as they are used in the configuration are

1. `Everyone` - default role assinged to any authenticated user without a dedicated role. This role has read access only to Grains (content) which are explicitely have corr. ACL entry (per default: none).
1. `Content_Consumer` - has readonly access to content Grains, not to data schema or extended functions.
1. `Content_Contributor` - can view and edit content Grains and view data schema.
1. `Schema_Manager` - can view and edit all Grains incl. schema (except for modifying built-in items), can modify ACLs, also publish, import and export Grains.
1. `Developer` - has full access required for development.
1. `Superuser` - has full access to everything, can modify other roles.

## Common Settings
- `Auth:Schema` - switches between `Basic` and `OIDC` (OAuth) scheme.

## Basic Authentication
- `Auth:Principals` - contains list of principals in the form of `"<USER_NAME>": "<PASSWORD_HASH>`" eligible to access the API. `USER_NAME` is some name of your chooing and `PASSWORD_HASH` is SHA-512 binary hash of the corr. user's password (use f.i. [SHA-512 tool](https://emn178.github.io/online-tools/sha512.html) to generate hashes). If you set `USER_NAME` to `*` arbitrary users can login with the specified password (this is preset in the development mode of standard distribution).
- `Auth:MapRoles` - here some or all of the users defined in `Auth:Princiapls` can be mapped to their roles within the MarBas system, without this mapping the users would be able to login but will have no access to any data or functions. The map has the form of `"<USER_NAME>": "<MARBAS_ROLE>"` where `USER_NAME` is authenticated user and `MARBAS_ROLE` one of roles listed [here](#marbas-roles). If `USER_NAME` is set to `*` all authenticated users without a mapped role will be assigned to the role in this entry.

## OAuth Authentication
MarBas databroker is designed and tested to work with any OAuth provider which supports OpenID Connect (OIDC). Most of the settings below can be learned by calling `https://<PROVIDER>/<REALM>/.well-known/openid-configuration`.
- `Auth:Authority` - **required** issuer of JWT tokens (OIDC equiv.: `issuer`).
- `Auth:ClientId` - **default: `databroker`** ID of the client as registered with your OAuth provider.
- `Auth:AuthorizationUrl` - **required** location of provider's login page (OIDC equiv.: `authorization_endpoint`).
- `Auth:TokenUrl` - **required** provider's API for issueing tokens (OIDC equiv.: `token_endpoint`).
- `Auth:LogoutUrl` - **optional** address to provider's page to logout users (OIDC equiv.: `end_session_endpoint`).
- `Auth:Flow` - **required** authentication flow to use, use `AuthorizationCode` for now as this one is tested.
- `Auth:PKCE` - **optional**, indicates if provider supports PKCE (Proof Key for Code Exchange), can be one of `NA`, `Available` or `Required`, set to `Available` if not sure.
- `Auth:UseTokenProxy` - **optional**, set to `true` if your provider (like Nextcloud) has too restrictive CORS settings, so `TokenUrl` has to be called server-to-server.
- `Auth:ClientSecret` - **optional** secret to authenticate confidential clients.
- `Auth:Scopes` - list of authentication scopes supported by provider, any scope can be marked as required. The list has to contain at least one required scope, for most providers it will be `"openid": true` (OIDC equiv.: `scopes_supported`).
- `Auth:ScopeSeparator` - **default: " "** character used to separate scopes in authorization requests.
- `Auth:MapClaimType` - **default: "role"**, if specified the JWT claim named here will be used to map to MarBas roles .
- `Auth:MapRoles` - is essentially the same as in [Basic Authentication](#basic-authentication), except for the JWT claim from `MapClaimType` being used as the map key. If the corr. claim sent by provider is populated with tokens like in [MarBas Roles](#marbas-roles) this map is optional.
- `Auth:TokenValidation:Set`, `Auth:TokenValidation:Unset` - **optional** fine tuning flags to apply / skip during JWT validation, can be one of `LogTokenId`, `LogValidationExceptions`, `RequireExpirationTime`, `RequireSignedTokens`, `RequireAudience`, `SaveSigninToken`, `TryAllIssuerSigningKeys`, `ValidateActor`, `ValidateAudience`, `ValidateIssuer`, `ValidateIssuerSigningKey`, `ValidateLifetime` or `ValidateTokenReplay`. Refer to https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters for documentation.

## OAuth Providers
OAuth provider configuration is unfortunately far too specific for each provider to describe it here. Important points to pay attention to:
- the provider should support [authrization code grant flow](https://oauth.net/2/grant-types/authorization-code/) and the client should be configured accordingly
- the provider should support at least `openid` [authorization scope](https://auth0.com/docs/get-started/apis/scopes/openid-connect-scopes), this scope should be active
- all client applications your use have to be registered with their [redirect URL](https://www.oauth.com/oauth2-servers/redirect-uris/) where they accept provider callbacks (for MarBas apps in development mode, you would register: https://localhost:7277/silo/index.html, http://localhost:5500/, https://localhost:7277/swagger/oauth2-redirect.html)
- the provider should support JWT claims like `role` or `groups` to allow role mapping within MarBas

### Ready-to-use OAuth Test Drive With [Keycloak](https://www.keycloak.org/)
The directory `<SOLUTION_ROOT>/doc/oauth/keycloak` contains pre-conficured docker definition and a settings file for MarBas databroker that can be used for testing OAuth authentication.

1. Copy `<SOLUTION_ROOT>/doc/oauth/keycloak/authsettings.json` into broker's working directory (`<SOLUTION_ROOT>/src/MarBasAPI` when running the solution).
1. Change into `<SOLUTION_ROOT>/doc/oauth/keycloak` directory.
1. Execute
	```sh
	docker compose up -d
	```
1. After container is spinned up you will find the Keycloak admin UI at http://localhost:3001/ (s. `docker-compose.yml` for admin credentials).

The provider in the container is set up with the realm `marbas` containing `databroker` client and following users:
- `reader` - having `Content_Consumer` role
- `editor` - having `Content_Contributor` role
- `manager` - having `Schema_Manager` role
- `developer` - having `Developer` role
- `root` - having `Superuser` role

All test users have password `b`.