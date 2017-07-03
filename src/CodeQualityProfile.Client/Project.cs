using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CodeQualityProfile.Client.FileSystem;
using CodeQualityProfile.Client.Processes;
using Microsoft.Extensions.Logging;

namespace CodeQualityProfile.Client
{
    public class Project : IProject
    {
        private readonly ILogger _logger = ApplicationLogging.CreateLogger<Project>();

        private readonly ICommandExecutor _commandExecutor;

        private readonly IXmlHelper _xmlHelper;

        public Project(string filePath, ICommandExecutor commandExecutor, IXmlHelper xmlHelper)
        {
            if (!Path.IsPathRooted(filePath))
            {
                throw new ArgumentOutOfRangeException(nameof(filePath), "The project file path must be absolute.");
            }

            FilePath = filePath;
            _commandExecutor = commandExecutor;
            _xmlHelper = xmlHelper;
        }

        public string FilePath { get; }

        public string AddOrUpdatePackage(string packageName, string version = null)
        {
            var versionArgument = string.Empty;
            if (!string.IsNullOrWhiteSpace(version))
            {
                versionArgument = $" -v {version}";
            }

            var result = _commandExecutor.ExecuteCommand("dotnet", $"add \"{FilePath}\" package {packageName}{versionArgument}");
            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException("dotnet command failed.");
            }

            return GetVersionFromStdout(result.StdOut, packageName);
        }

        public void MakePackageReferencePrivate(string packageName)
        {
            var xDocument = _xmlHelper.LoadFromFile(FilePath);
            var projectElement = xDocument.Element("Project");
            if (projectElement == null)
            {
                throw new InvalidDataException($"Could not locate the 'Project' element in the file {FilePath}.");
            }

            var referenceGroups = projectElement.Elements("ItemGroup").Select(
                i => i.Elements("PackageReference").Where(
                    r =>
                        {
                            var attribute = r.Attribute("Include");
                            return attribute != null && attribute.Value.Equals(packageName, StringComparison.OrdinalIgnoreCase);
                        })).Where(i => i.Any()).ToList();

            var packageReferences = referenceGroups.FirstOrDefault()?.ToList();
            if (packageReferences == null || packageReferences.Count < 1)
            {
                throw new InvalidDataException($"Could not find a package reference element for package '{packageName}' in file '{FilePath}'.");
            }

            if (referenceGroups.Count > 1 || packageReferences.Count > 1)
            {
                throw new InvalidDataException($"Found multiple package reference elements for package '{packageName}' in file '{FilePath}'.");
            }

            var packageReference = packageReferences.First();
            foreach (var privateAssetElement in packageReference.Elements("PrivateAssets"))
            {
                privateAssetElement.Remove();
            }

            packageReference.Add(new XElement("PrivateAssets", "All"));

            foreach (var excludeAssetsElement in packageReference.Elements("ExcludeAssets"))
            {
                excludeAssetsElement.Remove();
            }

            packageReference.Add(new XElement("ExcludeAssets", "contentFiles"));

            _xmlHelper.Save(xDocument, FilePath);
        }

        public void AddOrUpdateRuleSetReference(string relativeRuleSetPath)
        {
            var xDocument = _xmlHelper.LoadFromFile(FilePath);
            var projectElement = xDocument.Element("Project");
            if (projectElement == null)
            {
                throw new InvalidDataException($"Could not locate the 'Project' element in the file {FilePath}.");
            }

            var propertyGroups = projectElement.Elements("PropertyGroup").ToList();
            var newElement = new XElement("CodeAnalysisRuleSet", relativeRuleSetPath);

            if (propertyGroups.Count == 0)
            {
                _logger.LogTrace("No PropertyGroup found, adding a new with a single CodeAnalysisRuleSet element.");
                projectElement.Add(new XElement("PropertyGroup", newElement));
            }
            else
            {
                _logger.LogTrace("Found at least one PropertyGroup element.");
                var groupsWithRuleSet = propertyGroups.Where(g => g.Element("CodeAnalysisRuleSet") != null).ToList();
                if (groupsWithRuleSet.Count == 0)
                {
                    _logger.LogTrace("No PropertyGroup element with a CodeAnalysisRuleSet child found. Adding a new one to first PropertyGroup.");
                    propertyGroups.First().Add(newElement);
                }
                else
                {
                    _logger.LogTrace("Removing all CodeAnalysisRuleSet children from all PropertyGroup elements and adding a new one to the first PropertyGroup.");
                    foreach (var groupElement in groupsWithRuleSet)
                    {
                        foreach (var ruleSetElement in groupElement.Elements("CodeAnalysisRuleSet"))
                        {
                            ruleSetElement.Remove();
                        }
                    }

                    groupsWithRuleSet.First().Add(newElement);
                }
            }

            _xmlHelper.Save(xDocument, FilePath);
        }

        private string GetVersionFromStdout(string stdout, string packageName)
        {
            var regex = new Regex($"'{packageName}' version '(.*?)' (updated|added)");

            var regexMatch = regex.Matches(stdout);

            if (regexMatch.Count < 1 || !regexMatch[0].Success || regexMatch[0].Groups.Count < 2)
            {
                throw new InvalidOperationException("Cannot derive the actually installed or updated package version from standard output.") { Data = { { "stdout", stdout } } };
            }

            return regexMatch[0].Groups[1].Value;
        }
    }
}