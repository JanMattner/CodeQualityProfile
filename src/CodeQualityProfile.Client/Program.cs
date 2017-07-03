using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeQualityProfile.Client.FileSystem;
using CodeQualityProfile.Client.Processes;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CodeQualityProfile.Client
{
    public class Program
    {
        private const string Version = "0.2.0";

        protected Program()
        {
        }

        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build();
            var defaultPackageId = configuration["packageId"];
            var defaultExclusionPatterns = configuration.GetSection("exclusionPatterns")?.GetChildren()?.Select(s => s.Value).ToList() ?? new List<string>();
            var app = CreateApp(defaultPackageId, defaultExclusionPatterns);

            var result = 1;

            try
            {
                result = app.Execute(args);
            }
            catch (CommandParsingException e)
            {
                Console.Error.WriteLine(e.Message);
            }

            Environment.Exit(result);
        }

        private static CommandLineApplication CreateApp(string defaultPackageId, IReadOnlyCollection<string> defaultExclusionPatterns)
        {
            var app = new CommandLineApplication { FullName = "Code Quality Profile" };
            app.HelpOption("-? | -h | --help");
            app.VersionOption("-v | --version", Version);

            var solutionFolder = app.Argument("SOLUTION_FOLDER", "The base folder of the solution. If no folder is specified, the current working directory is used.");
            var version = app.Option(
                "-pv | --package-version",
                "The version of the package to be installed. If no version is specified, the latest stable version will be installed.",
                CommandOptionType.SingleValue);
            var package = app.Option(
                "-p | --package",
                $"The NuGet package ID to be installed as code quality profile. If no package ID is specified, {defaultPackageId} will be used.",
                CommandOptionType.SingleValue);

            var defaultExclusionPatternsHelpString = string.Empty;
            if (defaultExclusionPatterns.Count > 0)
            {
                defaultExclusionPatternsHelpString = $" If no pattern is specified, the following patterns will be used: {string.Join(", ", defaultExclusionPatterns)}";
            }

            var exclusionPatterns = app.Option(
                "-e | --exclude",
                $"Minimatch patterns to detect which project files should be excluded. Multiple values are allowed.{defaultExclusionPatternsHelpString}",
                CommandOptionType.MultipleValue);
            var verbosity = app.Option(
                "--verbosity",
                "The verbosity for logging. Allowed values: None, Error, Warning, Information, Debug, Trace. Default: Information.",
                CommandOptionType.SingleValue);

            app.ExtendedHelpText = "\n\nExample:\n\tdotnet cqp -p My.Package.ID -pv 1.4.0 -e \"**/*.UnitTests.csproj\" -e \"**/*.IntegrationTests.csproj\"";

            app.OnExecute(() => ExecuteApp(defaultPackageId, defaultExclusionPatterns, solutionFolder, version, package, verbosity, app, exclusionPatterns));
            return app;
        }

        private static int ExecuteApp(
            string defaultPackageId,
            IReadOnlyCollection<string> defaultExclusionPatterns,
            CommandArgument solutionFolder,
            CommandOption version,
            CommandOption package,
            CommandOption verbosity,
            CommandLineApplication app,
            CommandOption exclusionPatterns)
        {
            var solutionFolderValue = solutionFolder.Value;
            if (string.IsNullOrWhiteSpace(solutionFolderValue))
            {
                solutionFolderValue = Directory.GetCurrentDirectory();
            }

            // ensure that the solution folder path is absolute
            solutionFolderValue = Path.GetFullPath(solutionFolderValue);

            string versionValue = null;
            if (version.HasValue())
            {
                versionValue = version.Value();
            }

            var packageValue = defaultPackageId;
            if (package.HasValue())
            {
                packageValue = package.Value();
            }

            var logLevel = LogLevel.Information;
            if (verbosity.HasValue() && !Enum.TryParse(verbosity.Value(), true, out logLevel))
            {
                app.ShowHelp();
                return 1;
            }

            var exclusionPatternsValue = defaultExclusionPatterns;
            if (exclusionPatterns.HasValue())
            {
                exclusionPatternsValue = exclusionPatterns.Values;
            }

            ApplicationLogging.LoggerFactory.AddConsole(logLevel);
            var logger = ApplicationLogging.CreateLogger<Program>();

            logger.LogInformation($"Solution Base Folder: {solutionFolderValue}");
            logger.LogInformation($"Code Quality Package: {packageValue}");
            logger.LogInformation($"Package Target Version: {versionValue ?? "Latest"}");

            if (exclusionPatterns.HasValue())
            {
                logger.LogInformation($"Exclude projects matching: {string.Join(", ", exclusionPatternsValue)}");
            }

            if (versionValue == null || !versionValue.Contains("*"))
            {
                try
                {
                    var projectFactory = new ProjectFactory(new CommandExecutor(solutionFolderValue), new XmlHelper());
                    var codeQualitySolution = new CodeQualitySolution(solutionFolderValue, new DirectoryHelper(), new NuGetPathHelper(), new FileHelper(), projectFactory);
                    codeQualitySolution.AddOrUpdatePackage(packageValue, versionValue, exclusionPatternsValue);
                    logger.LogInformation("Done.");
                    return 0;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception.Message);
                    return 1;
                }
            }

            logger.LogError("Version Number with Wildcard ('*') is not supported.");
            return 1;
        }
    }
}