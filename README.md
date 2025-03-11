# marbas-databroker
![Runs on Windows](https://img.shields.io/badge/_%E2%9C%94-Win-black) ![Runs on MacOS](https://img.shields.io/badge/_%E2%9C%94-Mac-black) ![Runs on Linux](https://img.shields.io/badge/_%E2%9C%94-Linux-black) ![Tool](https://img.shields.io/badge/.Net-8-lightblue)

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

API is then available at https://localhost:7277/swagger/index.html. Endpoints require authentication, for testing purposes dummy basic auth is turned on. In Swagger go to "Authorize" and login using arbitrary user name with password "*b*"

## Using NuGet Packages
Packages needed to run databroker are published on https://www.nuget.org (keyword: "CraftedSolutions.MarBas"), for utilization example s. https://github.com/Crafted-Solutions/marbas-databroker-pgsql.

## Contributing
All contributions to development and error fixing are welcome. Please always use `develop` branch for forks and pull requests, `main` is reserved for stable releases and critical vulnarability fixes only.