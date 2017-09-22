using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CodeQualityProfile.Client.FileSystem;
using Microsoft.Extensions.Logging;
using Minimatch;

namespace CodeQualityProfile.Client
{
    public class CodeQualitySolution : ICodeQualitySolution
    {
        private readonly ILogger _logger = ApplicationLogging.CreateLogger<CodeQualitySolution>();

        private readonly IDirectoryHelper _directoryHelper;

        private readonly IFileHelper _fileHelper;

        private readonly INuGetPathHelper _nuGetPathHelper;

        private readonly List<IProject> _projects = new List<IProject>();

        private readonly string _basePath;

        private readonly string _solutionFileName;

        public CodeQualitySolution(string basePath, IDirectoryHelper directoryHelper, INuGetPathHelper nuGetPathHelper, IFileHelper fileHelper, IProjectFactory projectFactory)
        {
            _directoryHelper = directoryHelper;
            _nuGetPathHelper = nuGetPathHelper;
            _fileHelper = fileHelper;

            if (!Path.IsPathRooted(basePath))
            {
                throw new ArgumentOutOfRangeException(nameof(basePath), "The base path of the solution must be absolute.");
            }

            _basePath = basePath;

            var solutionFilePath = _directoryHelper.GetFiles(".", "*.sln", SearchOption.TopDirectoryOnly, _basePath).FirstOrDefault();
            _solutionFileName = Path.GetFileName(solutionFilePath);
            if (HasSolutionFile)
            {
                _logger.LogInformation($"Using solution file: {_solutionFileName}");
            }
            else
            {
                _logger.LogWarning("No solution file found.");
            }

            var projectFiles = _directoryHelper.GetFiles(".", "*.csproj", SearchOption.AllDirectories, _basePath);
            foreach (var projectFile in projectFiles)
            {
                _projects.Add(projectFactory.CreateProject(Path.GetFullPath(Path.Combine(_basePath, projectFile))));
            }
        }

        private bool HasSolutionFile => !string.IsNullOrWhiteSpace(_solutionFileName);

        public void AddOrUpdatePackage(string codeQualityProfilePackageName, string version = null, IReadOnlyCollection<string> exclusionPatterns = null)
        {
            if (exclusionPatterns == null)
            {
                exclusionPatterns = new string[0];
            }

            if (_projects.Count < 1)
            {
                throw new InvalidOperationException("The solution contains no projects.");
            }

            var notExcludedProjects = GetNotExcludedProjects(exclusionPatterns);

            _logger.LogInformation($"Add or update NuGet package '{codeQualityProfilePackageName}' for projects ...");
            var targetVersion = AddOrUpdateNuGetPackage(notExcludedProjects, codeQualityProfilePackageName, version);

            if (version == null)
            {
                _logger.LogInformation($"Installed latest version {targetVersion}.");
            }

            _logger.LogInformation("Copy files from package to solution base folder ...");
            var localRuleSetFilePath = CopyFiles(codeQualityProfilePackageName, targetVersion);

            _logger.LogInformation("Adjust project files ...");
            AdjustProjects(notExcludedProjects, codeQualityProfilePackageName, localRuleSetFilePath);
        }

        private void AdjustProjects(IReadOnlyCollection<IProject> projects, string codeQualityProfilePackageName, string localRuleSetFilePath)
        {
            foreach (var project in projects)
            {
                project.MakePackageReferencePrivate(codeQualityProfilePackageName);
                project.AddOrUpdateRuleSetReference(GetRelativePath(project.FilePath, localRuleSetFilePath));
            }
        }

        private string CopyFiles(string codeQualityProfilePackageName, string targetVersion)
        {
            var nuGetPackagePath = _nuGetPathHelper.GetPackageContentPath(codeQualityProfilePackageName, targetVersion);
            var ruleSetFilePath = _directoryHelper.GetFiles(nuGetPackagePath, "*.ruleset").FirstOrDefault();
            if (string.IsNullOrWhiteSpace(ruleSetFilePath))
            {
                throw new InvalidOperationException($"Could not locate a *.ruleset file in the package folder '{nuGetPackagePath}'.");
            }

            var ruleSetFileName = Path.GetFileName(ruleSetFilePath);
            var localRuleSetFilePath = Path.Combine(_basePath, ruleSetFileName);
            _logger.LogInformation($"Moving file {ruleSetFilePath} to {localRuleSetFilePath} ...");
            _fileHelper.Copy(ruleSetFilePath, localRuleSetFilePath, true);

            if (HasSolutionFile)
            {
                var dotsettingsFilePath = _directoryHelper.GetFiles(nuGetPackagePath, "*.DotSettings").FirstOrDefault();
                if (string.IsNullOrWhiteSpace(dotsettingsFilePath))
                {
                    throw new InvalidOperationException($"Could not locate a *.DotSettings file in the package folder '{nuGetPackagePath}'.");
                }

                var localDotDettingsFilePath = Path.Combine(_basePath, $"{_solutionFileName}.DotSettings");
                _logger.LogInformation($"Moving file {dotsettingsFilePath} to {localDotDettingsFilePath} ...");
                _fileHelper.Copy(dotsettingsFilePath, localDotDettingsFilePath, true);
            }

            return localRuleSetFilePath;
        }

        private string AddOrUpdateNuGetPackage(IReadOnlyCollection<IProject> projects, string codeQualityProfilePackageName, string version)
        {
            var targetVersion = version;
            foreach (var project in projects)
            {
                _logger.LogTrace($"Add or update NuGet package for {project} ...");
                var installedVersion = project.AddOrUpdatePackage(codeQualityProfilePackageName, version);
                if (string.IsNullOrWhiteSpace(installedVersion))
                {
                    throw new InvalidOperationException(
                        $"Could not add or update the package '{codeQualityProfilePackageName}' for project '{project.FilePath}' since no actual installed version could be inferred.");
                }

                if (string.IsNullOrWhiteSpace(targetVersion))
                {
                    targetVersion = installedVersion;
                }
                else if (targetVersion != installedVersion)
                {
                    throw new InvalidOperationException(
                        $"The installed version '{installedVersion}' for project '{project.FilePath}' does not match the target version '{targetVersion}'. Please resolve this conflict manually.");
                }
            }

            return targetVersion;
        }

        private IReadOnlyCollection<IProject> GetNotExcludedProjects(IReadOnlyCollection<string> exclusionPatterns)
        {
            if (exclusionPatterns == null || exclusionPatterns.Count < 1)
            {
                return _projects;
            }

            var notExcludedProjects = new List<IProject>();

            foreach (var project in _projects)
            {
                var excluded = false;
                foreach (var excludedProject in exclusionPatterns)
                {
                    if (Minimatcher.Check(project.FilePath, excludedProject, new Options { AllowWindowsPaths = true, NoCase = true }))
                    {
                        _logger.LogDebug($"Ignoring project '{project.FilePath}' as it matches the exclusion pattern {excludedProject}");
                        excluded = true;
                        break;
                    }
                }

                if (excluded)
                {
                    continue;
                }

                notExcludedProjects.Add(project);
            }

            return notExcludedProjects;
        }

        private string GetRelativePath(string from, string to)
        {
            var uriFrom = new Uri(from);
            var uriTo = new Uri(to);
            return uriFrom.MakeRelativeUri(uriTo).OriginalString.Replace('/', '\\');
        }
    }
}