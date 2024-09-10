# marbas-databroker
Data schema management libraries for MarBas system.

![logo](src/MarBasBrokerEngineSQLite/Resources/marbas.png)

## Building
Execute in the solution directory
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
1. Generate your GitHub personal access token [here](https://github.com/login?return_to=https%3A%2F%2Fgithub.com%2Fsettings%2Ftokens) with **read:packages** permission.
1. Add https://nuget.pkg.github.com/Crafted-Solutions/index.json repository to your **local** `nuget.config`:
    ```xml
    <packageSources>
        <add key="crafted-solutions" value="https://nuget.pkg.github.com/Crafted-Solutions/index.json"/>
    </packageSources>
    <packageSourceCredentials>
        <crafted-solutions>
            <add key="Username" value="YOUR_USER_NAME"/>
            <add key="ClearTextPassword" value="YOUR_PACKAGE_TOKEN"/>
        </crafted-solutions>
    </packageSourceCredentials>
    ```
    Alternatively run this command
    ```sh
    dotnet nuget add source https://nuget.pkg.github.com/Crafted-Solutions/index.json -n crafted-solutions -u YOUR_USER_NAME -p YOUR_PACKAGE_TOKEN --store-password-in-clear-text
    ```
    Alternatively in Visual Studio go to “Tools” -> “Options” -> “NuGet Package Manager” -> “Package Sources” and add the repository as new source.
    
    *DON'T COMMIT ANY CONFIGURATION CONTAINING TOKENS!*