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
- `Auth:Audience` - **optional** audience the tokens are issued for.
- `Auth:AuthorizationUrl` - **required** location of provider's login page (OIDC equiv.: `authorization_endpoint`).
- `Auth:TokenUrl` - **required** provider's API for issueing tokens (OIDC equiv.: `token_endpoint`).
- `Auth:LogoutUrl` - **optional** address to provider's page to logout users (OIDC equiv.: `end_session_endpoint`).
- `Auth:Flow` - **required** authentication flow to use, use `AuthorizationCode` for now as this one is tested.
- `Auth:PKCE` - **optional**, indicates if provider supports PKCE (Proof Key for Code Exchange), can be one of `NA`, `Available` or `Required`, set to `Available` if not sure.
- `Auth:UseTokenProxy` - **optional**, set to `true` if your provider (like Nextcloud) has too restrictive CORS settings, so `TokenUrl` has to be called server-to-server.
- `Auth:ClientSecret` - **optional** secret to authenticate confidential clients.
- `Auth:RequireHttpsMetadata` - **default: `true`** if set to `false` to allow unencrypted connections to OAuth APIs - __never use in production__.
- `Auth:Scopes` - list of authentication scopes supported by provider, any scope can be marked as required. The list has to contain at least one required scope, for most providers it will be `"openid": true` (OIDC equiv.: `scopes_supported`).
- `Auth:ScopeSeparator` - **default: " "** character used to separate scopes in authorization requests.
- `Auth:MapClaimType` - **default: "role"**, if specified the JWT claim named here will be used to map to MarBas roles .
- `Auth:MapRoles` - is essentially the same as in [Basic Authentication](#basic-authentication), except for the JWT claim from `MapClaimType` being used as the map key. If the corr. claim sent by provider is populated with tokens like in [MarBas Roles](#marbas-roles) this map is optional.
- `Auth:TokenValidation:Set`, `Auth:TokenValidation:Unset` - **optional** fine tuning flags to apply / skip during JWT validation, can be one of `LogTokenId`, `LogValidationExceptions`, `RequireExpirationTime`, `RequireSignedTokens`, `RequireAudience`, `SaveSigninToken`, `TryAllIssuerSigningKeys`, `ValidateActor`, `ValidateAudience`, `ValidateIssuer`, `ValidateIssuerSigningKey`, `ValidateLifetime` or `ValidateTokenReplay`. Refer to https://learn.microsoft.com/en-us/dotnet/api/microsoft.identitymodel.tokens.tokenvalidationparameters for documentation.

*Note: the host where MarBas is running has to be able to establish outgoing connections to `Authority`, especially if you set `UseTokenProxy`.*

## OAuth Providers
OAuth provider configuration is unfortunately far too specific for each provider to describe it here. Important points to pay attention to:
- the provider should support [authrization code grant flow](https://oauth.net/2/grant-types/authorization-code/) and the client should be configured accordingly
- the provider should support at least `openid` [authorization scope](https://auth0.com/docs/get-started/apis/scopes/openid-connect-scopes), this scope should be active
- all client applications your use have to be registered with their [redirect URL](https://www.oauth.com/oauth2-servers/redirect-uris/) where they accept provider callbacks (for MarBas apps in development mode, you would register: https://localhost:7277/silo/index.html, http://localhost:5500/, https://localhost:7277/swagger/oauth2-redirect.html)
- the provider should support JWT claims like `role` or `groups` to allow role mapping within MarBas

### Ready-to-use Test Drive With [Keycloak](https://www.keycloak.org/)
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

### Test Drive With [Authentik](https://goauthentik.io/)
The directory `<SOLUTION_ROOT>/doc/oauth/authentik` contains necessary files to set up an Authentik test provider. There are several setup options, depending on your environment. In all variants you will need `<SOLUTION_ROOT>/doc/oauth/authentik/authsettings.json` in the broker's working directory. The mentioned below `marbas.yml` creates the same users as described in [Keycloak Test Drive](#ready-to-use-test-drive-with-keycloak) when imported.

#### Authentik Standalone Docker Container
This option is easiest one to use and is applicable on hosts __without pre-existing PostgreSQL__ instances.

1. Change into `<SOLUTION_ROOT>/doc/oauth/authentik` directory.
1. Execute
	```sh
	docker compose -f docker-compose-standalone.yml up -d
	```
1. After containers are spinned up open http://localhost:9000/if/flow/initial-setup/ and follow the instructions.
1. In the admin console go to "Flows & Stages" -> "Flows" and import `<SOLUTION_ROOT>/doc/oauth/authentik/marbas.yml`.

#### Authentik Docker Container Using Existing Database
If you already have a PostgreSQL instance running on the same host, you should give this option a go.

1. In your PostgreSQL instance execute
	```sql
	CREATE USER authentik WITH LOGIN CREATEDB CREATEROLE REPLICATION PASSWORD 'authentik';
	CREATE DATABASE authentik WITH OWNER authentik;
	```
1. Change into `<SOLUTION_ROOT>/doc/oauth/authentik` directory.
1. Execute
	```sh
	docker compose -f docker-compose-shared-pg.yml up -d
	```
1. After containers are spinned up open http://localhost:9000/if/flow/initial-setup/ and follow the instructions.
1. In the admin console go to "Flows & Stages" -> "Flows" and import `<SOLUTION_ROOT>/doc/oauth/authentik/marbas.yml`.

#### Existing Authentik Instance
1. In the Authentik admin console go to "Flows & Stages" -> "Flows" and import `<SOLUTION_ROOT>/doc/oauth/authentik/marbas.yml`.
1. Modify URLs in `authsettings.json` in the broker's directory to point to location of your Authentik host.

### Test Drive With [Nextcloud](https://nextcloud.com/)
Nextcloud is capable of being OIDC provider with the help from https://github.com/H2CK/oidc. From within directory `<SOLUTION_ROOT>/doc/oauth/nextcloud` you can run Nextcloud docker container and authenticate MarBas against it.

1. Copy `<SOLUTION_ROOT>/doc/oauth/nextcloud/authsettings.json` into broker's working directory (`<SOLUTION_ROOT>/src/MarBasAPI` when running the solution).
1. Change into `<SOLUTION_ROOT>/doc/oauth/nextcloud` directory.
1. Execute
	```sh
	docker compose up -d
	```
1. After container is spinned up open http://localhost:8066 and follow the instructions (you can skip recommended apps installation - they are not needed).
1. With Nextcloud up and running make sure that all files in the directory `<SOLUTION_ROOT>/doc/oauth/nextcloud/scripts` are executable by all users then run
	```sh
	docker exec nextcloud /bin/bash -c "/scripts/marbas-oidc.sh"
	```
1. If all step complete successfully a file `<SOLUTION_ROOT>/doc/oauth/nextcloud/scripts/marbas-oidc.json` would be generated with OIDC client configuration. Open the file and copy the string under `client_id`, replace `<YOUR_CLIENT_ID>` in `authsettings.json` with it.

The configuration would provide following users:
- `mb-reader` - having `Content_Consumer` role
- `mb-editor` - having `Content_Contributor` role
- `mb-manager` - having `Schema_Manager` role
- `mb-developer` - having `Developer` role
- `mb-root` - having `Superuser` role

All test users have password `dummypass_b`.
