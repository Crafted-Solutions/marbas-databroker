# marbas-databroker
![Cross-Platform Compatibility](https://jstools.dev/img/badges/os-badges.svg) ![Tool](https://img.shields.io/badge/.Net-8-lightblue)

Data schema management libraries for MarBas system.

![logo](src/MarBasBrokerEngineSQLite/Resources/marbas.png)

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
Add https://nuget.pkg.github.com/Crafted-Solutions/index.json repository to your **local** `nuget.config`
```xml
<packageSources>
    <add key="crafted-solutions" value="https://nuget.pkg.github.com/Crafted-Solutions/index.json"/>
</packageSources>
```
Alternatively run this command
```sh
dotnet nuget add source https://nuget.pkg.github.com/Crafted-Solutions/index.json -n crafted-solutions
```
Alternatively in Visual Studio go to “Tools” -> “Options” -> “NuGet Package Manager” -> “Package Sources” and add the repository as new source.

## Contributing
All contributions to development and error fixing are welcome. Please always use `develop` branch for forks and pull requests, `main` is reserved for stable releases and critical vulnarability fixes only.