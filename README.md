# Code Quality Profile

The Code Quality Profile makes it easy to distribute and apply rules for the static code and style analysis for .NET Core projects using Roslyn Analyzers. The profile is distributed as a NuGet package and can be installed or updated with a simple client tool as dotnet command.

## Getting Started

### Prerequisites

You need to install Visual Studio 2017 or at least dotnet 1.0.1 in order to properly create or consume the Code Quality Profile package and client tool.

The `dotnet` command must be available from the command line (in the global PATH). If you need futher infomation for using the dotnet core CLI you can get them visiting the GitHub dotnet CLI [page](https://github.com/dotnet/cli).

### Install the Client Tool

- Download the dotnet-cqp.zip of the latest release and unzip it to some local folder on your developer machine.
- Add the path of the unzipped dotnet-cqp folder to your global PATH variable.
- For Linux users: you might need to add execution permission for the dotnet-cqp.sh file, e.g. `chmod a+x dotnet-cqp.sh`
- Reload the PATH environment variable or open a new shell. You can use the tool with `dotnet cqp`.

### Use the Client Tool

```
Usage:  [arguments] [options]

Arguments:
  SOLUTION_FOLDER  The base folder of the solution. If no folder is specified, the current working directory is used.

Options:
  -? | -h | --help         Show help information
  -v | --version           Show version information
  -pv | --package-version  The version of the package to be installed. If no version is specified, the latest stable version will be installed.
  -p | --package           The NuGet package ID to be installed as code quality profile. If no package ID is specified, AIT.CodeQualityProfile will be used.
  -e | --exclude           Minimatch patterns to detect which project files should be excluded. Multiple values are allowed. If no pattern is specified, the following patterns will be used: *.*Test.csproj, *.*Tests.csproj
  --verbosity              The verbosity for logging. Allowed values: None, Error, Warning, Information, Debug, Trace. Default: Information.


Example:
        dotnet cqp -p My.Package.ID -pv 1.4.0 -e "**/*.UnitTests.csproj" -e "**/*.IntegrationTests.csproj"
```

The tool does:

- install the Code Quality Profile NuGet package for each project (*.csproj files) found in any subfolder of the `SOLUTION_FOLDER`
- Copy the `ruleset` (for configuring the actual rules in the Roslyn analyzers) and `DotSettings` (for configuring ReSharper to fit the chosen rules) in the `SOLUTION_FOLDER`
- Add the `ruleset` as `CodeAnalysisRuleset` in each project
- Fix the package references of the Code Quality Profile for each project such that the package is essentially a developer dependency only (i.e. its dependencies and content files are consumed but do not flow to the next project)

To persist the added `ruleset` and `DotSettings` files, you should add them to your source control.

Note that the tool uses the standard `dotnet` functionality from the command line. Only packages that are available in the NuGet package sources configured in the user's `NuGet.config` can be consumed. If the package sources need authorization, the package source credentials need to be defined in the config as well.

On Windows, you can usually find the file at: `%APPDATA%\NuGet\NuGet.config`

On Linux, you can usually find the file at: `~/.nuget/NuGet/NuGet.Config`

Without a specific version, the latest stable package will be installed. Pre-release versions can only be installed by specifying them explicitly.

The client tool usually needs to be installed only once on a developer machine - new package versions can be installed and updated with that same client tool!

## Add VSTS nuget feed

NuGet Documentation for dotnet: [Link](https://www.visualstudio.com/en-us/docs/package/nuget/dotnet-exe)

You can either add the VSTS NuGet feed and crendentials with the nuget command or you can edit the global nuget config.

NuGet command:
```
nuget.exe sources add -name {feed name} -source {feed URL} -username {user@domain.com} -password {VSTS_PERSONAL_ACCESS_TOKEN} -StorePasswordInClearText
```
NuGet Config:
```xml
<configuration>
  <packageSources>
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    <add key="{feed name}" value="{feed url}" />
  </packageSources>
  <packageSourceCredentials>
    <TestFeed>
      <add key="Username" value="user@domain.com" />
      <add key="ClearTextPassword" value="VSTS_PERSONAL_ACCESS_TOKEN" />
    </TestFeed>
  </packageSourceCredentials>
</configuration>
```

## Custom Code Quality Profile

You can easily customize the used Roslyn Analyzers, the `ruleset` and `DotSettings` file. The package name and default package to be installed with the client tool can be configured as well.

### Profile Package Customization

Customize the package project at `src/CodeQualityProfile.Policy`.

All dependencies, particularly the Roslyn Analyzers, will be part of the package and installed in the target projects. Make sure to keep the `ruleset` and `DotSettings` files up to date!

### Client Customization

Customize the client project at `src/CodeQualityProfile.Client`.

Configurations in `appsettings.json`:

- `packageId`: default NuGet package ID to be used if no `--package` option is supplied.
- `exclusionPatterns`: default patterns for project files that will be excluded.

### Build and Test Client

```
dotnet restore src
dotnet build src -c Release
```

With the following command you can test the CLI tool and find the results trx-file at `src/CodeQualityProfile.Client.UnitTests/TestResults/results.trx`:

```
dotnet test src/CodeQualityProfile.Client.UnitTests/CodeQualityProfile.Client.UnitTests.csproj -c Release --no-build --logger "trx;LogFileName=results.trx"
```

You can consume this results file e.g. in a VSTS build.

### Create Profile Package

Open the file `PACKAGE_VERSION` and adjust the version number as desired.

Run the following command with your `PACKAGE_ID`:

- Windows: `.\pack.bat PACKAGE_ID`
- Linux: `.\pack.sh PACKAGE_ID`
    - you might need to first change the execution permission for the script file, e.g. `chmod a+x pack.sh`

The nupkg-file will be placed in the `build/package` subfolder.

### Create Client Redistributable Package

Run the following command:

```
dotnet publish src/CodeQualityProfile.Client -c Release -o ../../build/dotnet-cqp
```

You will find all the necessary files in the `build/dotnet-cqp` folder. Just zip it and distribute it to your developers!