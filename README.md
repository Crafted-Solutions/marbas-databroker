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
Per default the binary starts production HTTP server (no SSL) on a free port (mostly 5000), i.e. the API endpoints would be reachable via http://localhost:5000/api/marbas. In production mode Swagger is disabled and the only configured user is `reader` with password "*Change_Me*" (can be set in `appsettings.json`). We strongly recommend not using basic authentication with sensitive data, especially when the API is publically accessible - in the future releases we will provide more secure authentication modules.

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

## Using NuGet Packages
Packages needed to run databroker are published on https://www.nuget.org (keyword: "CraftedSolutions.MarBas"), for utilization example s. https://github.com/Crafted-Solutions/marbas-databroker-pgsql.

## Contributing
All contributions to development and error fixing are welcome. Please always use `develop` branch for forks and pull requests, `main` is reserved for stable releases and critical vulnarability fixes only.