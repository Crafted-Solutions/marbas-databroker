# marbas-databroker
![Runs on Windows](https://img.shields.io/badge/_%E2%9C%94-Win-black) ![Runs on MacOS](https://img.shields.io/badge/_%E2%9C%94-Mac-black) ![Runs on Linux](https://img.shields.io/badge/_%E2%9C%94-Linux-black) ![Tool](https://img.shields.io/badge/.Net-8-lightblue) [<img src="https://img.shields.io/github/v/release/Crafted-Solutions/marbas-databroker" title="Latest">](../../releases/latest)

Data schema management libraries for MarBas system.

![logo](doc/marbas.png)

## Building
For building the application .NET SDK 8.x is required (recommended: Visual Studio or Visual Studio Code).

After cloning the repository you can either open the solution `MarBasBroker.sln` in your IDE and hit "Build" or open the terminal in the solution directory and execute
```sh
dotnet build
```

## Running
Execute in the solution directory
```sh
dotnet run --project src/MarBasAPI/MarBasAPI.csproj
```

Swagger test app is then available at https://localhost:7277/swagger/index.html and API - at https://localhost:7277/api/marbas. Endpoints require authentication, for testing purposes dummy basic auth is turned on. In Swagger go to "Authorize" and login using arbitrary user name with password "*b*".

Aleternatively you can download pre-built binary archive of your choice from [Releases](../../releases/latest), extract it somewhere on your computer, change into that directory and run in the terminal
```sh
./MarBasAPI
```
Per default the binary starts production HTTP server (no SSL) on a free port (mostly 5000), i.e. the API endpoints would be reachable via http://localhost:5000/api/marbas. In production mode Swagger is disabled and the only configured user is `reader` with password "*Change_Me*" (can be set in `appsettings.json`). We strongly recommend not using basic authentication with sensitive data, especially when the API is publically accessible - from version 0.1.19 on the application supports OAuth (s. [Confuguring Authentication](doc/Authentication.md)).

If you wish that the pre-built executable behaves exactly like the project run by DotNet, set the following environment variables before running `MarBasAPI`
```sh
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=https://localhost:7277
```

### Example Use
```sh
curl -u reader:b "https://localhost:7277/api/marbas/Tree/**"
curl -u reader:b "https://localhost:7277/api/marbas/Role/Current"
```

## Configuration
Like other ASP.Net applications configuration options are stored in `appsettings.json` and resp. `appsettings.<ASPNETCORE_ENVIRONMENT>.json` files in the working directory. Most of the options are a subset of standard .Net settings and should only be changed by experienced developers.
### Configuration: BrokerProfile
In this section DB profile is configured, available options are dependent from actual DB backend used - the basis version uses SQLite database.
- `BrokerProfile:DataSource` - path pointing to the DB file (default is `Data/marbas.sqlite`).
- `BrokerProfile:Pooling` - if `false` connection pooling is off (default is on).
### Configuration: Cors
This section provides CORS options for the exposed API.
- `Cors:Enabled` - if `true` enables CORS (default is `false`), note that diabling CORS would disable access to the API from any host (domain) other than the same one where the API is running.
- `Cors:Policies` - list of CORS policies to implement (for the time being only `Default` is actually used).
- `Cors:Policies:<#>:Name` - use `Default`.
- `Cors:Policies:<#>:AllowedOrigins` - either `*` (allow any origin) or comma separated list of origins to match the value sent by a client in the `ORIGIN` header, f.i. `http://localhost:5500,https://localhost:5500`. If the specified origin has an internationalized domain name (IDN), the punycoded value is used. If the origin specifies a default port (e.g. 443 for HTTPS or 80 for HTTP), this will be dropped as part of normalization.
- `Cors:Policies:<#>:AllowedMethods` - either `*` (all HTTP methods are allowed) or a comma separated list of methods to allow like `GET,POST`. Only configure if you know what you are doing.
- `Cors:Policies:<N>:AllowedHeaders` - use to restrict specific headers browsers are allowed to send to the API. Usually you would leave it at `*` (all headers allowed).
- `Cors:Policies:<#>:AllowCredentials` - should be always `true`.
### Configuration: StaticFiles
- `StaticFiles:Enabled` - if `true` files found in the `wwwroot` directory are served to the browser statically. It can be used for example to deploy your frontend client application together with the MarBas API.
### Configuration: Auth
- In this section authentication methods used by the API can be configured, for details s. [Confuguring Authentication](doc/Authentication.md).

## Using NuGet Packages
Packages needed to run databroker are published on https://www.nuget.org (keyword: "CraftedSolutions.MarBas"), for utilization example s. https://github.com/Crafted-Solutions/marbas-databroker-pgsql.

## Contributing
All contributions to development and error fixing are welcome. Please always use `develop` branch for forks and pull requests, `main` is reserved for stable releases and critical vulnarability fixes only.