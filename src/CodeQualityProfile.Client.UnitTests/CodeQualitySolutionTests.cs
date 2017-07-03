using System.IO;
using CodeQualityProfile.Client.FileSystem;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CodeQualityProfile.Client.UnitTests
{
    [TestClass]
    public class CodeQualitySolutionTests
    {
        [TestMethod]
        public void CodeQualitySolution_AddOrUpdatePackage_ShouldApplyForAllProjects()
        {
            var directoryHelperMock = new Mock<IDirectoryHelper>();
            var nuGetPathHelperMock = new Mock<INuGetPathHelper>();
            var fileHelperMock = new Mock<IFileHelper>();
            var projectFactoryMock = new Mock<IProjectFactory>();

            var project1Mock = new Mock<IProject>();
            var project2Mock = new Mock<IProject>();

            projectFactoryMock.Setup(m => m.CreateProject(It.IsRegex(".*project1.csproj$"))).Returns(project1Mock.Object);
            projectFactoryMock.Setup(m => m.CreateProject(It.IsRegex(".*project2.csproj$"))).Returns(project2Mock.Object);

            project1Mock.SetupGet(m => m.FilePath).Returns("C:\\foo\\project1\\project1.csproj");
            project2Mock.SetupGet(m => m.FilePath).Returns("C:\\foo\\project2\\project2.csproj");

            project1Mock.Setup(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>())).Returns("PackageReference for package 'mypackage' version '0.1.2' added in file");
            project2Mock.Setup(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>())).Returns("PackageReference for package 'mypackage' version '0.1.2' added in file");

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("csproj"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\foo\\project1\\project1.csproj", "C:\\foo\\project2\\project2.csproj" });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("sln"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\foo\\solution.sln" });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("ruleset"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\nuget\\myruleset.ruleset" });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("DotSettings"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\nuget\\mysettings.DotSettings" });

            nuGetPathHelperMock.Setup(m => m.GetPackageContentPath(It.IsAny<string>(), It.IsAny<string>())).Returns("C:\\nuget");

            var codeQualitySolution = new CodeQualitySolution("C:\\foo", directoryHelperMock.Object, nuGetPathHelperMock.Object, fileHelperMock.Object, projectFactoryMock.Object);

            codeQualitySolution.AddOrUpdatePackage("mypackage");

            project1Mock.Verify(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>()));
            project1Mock.Verify(m => m.AddOrUpdateRuleSetReference(It.IsAny<string>()));
            project1Mock.Verify(m => m.MakePackageReferencePrivate(It.IsAny<string>()));
            project2Mock.Verify(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>()));
            project2Mock.Verify(m => m.AddOrUpdateRuleSetReference(It.IsAny<string>()));
            project2Mock.Verify(m => m.MakePackageReferencePrivate(It.IsAny<string>()));
        }

        [DataTestMethod]
        [DataRow("**/*.UnitTests.csproj", "C:\\foo\\project1\\project1.csproj", "C:\\foo\\project1.UnitTests\\project1.UnitTests.csproj")]
        public void CodeQualitySolution_AddOrUpdatePackageWithExcludedProjects_ShouldApplyForNotExcludedProjects(string excludePattern, string includedProjectPath, string excludedProjectPath)
        {
            var directoryHelperMock = new Mock<IDirectoryHelper>();
            var nuGetPathHelperMock = new Mock<INuGetPathHelper>();
            var fileHelperMock = new Mock<IFileHelper>();
            var projectFactoryMock = new Mock<IProjectFactory>();

            var project1Mock = new Mock<IProject>();
            var project2Mock = new Mock<IProject>();

            projectFactoryMock.Setup(m => m.CreateProject(includedProjectPath)).Returns(project1Mock.Object);
            projectFactoryMock.Setup(m => m.CreateProject(excludedProjectPath)).Returns(project2Mock.Object);

            project1Mock.SetupGet(m => m.FilePath).Returns(includedProjectPath);
            project2Mock.SetupGet(m => m.FilePath).Returns(excludedProjectPath);

            project1Mock.Setup(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>())).Returns("PackageReference for package 'mypackage' version '0.1.2' added in file");
            project2Mock.Setup(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>())).Returns("PackageReference for package 'mypackage' version '0.1.2' added in file");

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("csproj"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { includedProjectPath, excludedProjectPath });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("sln"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\foo\\solution.sln" });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("ruleset"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\nuget\\myruleset.ruleset" });

            directoryHelperMock.Setup(m => m.GetFiles(It.IsAny<string>(), It.IsRegex("DotSettings"), It.IsAny<SearchOption>(), It.IsAny<string>()))
                .Returns(new string[] { "C:\\nuget\\mysettings.DotSettings" });

            nuGetPathHelperMock.Setup(m => m.GetPackageContentPath(It.IsAny<string>(), It.IsAny<string>())).Returns("C:\\nuget");

            var codeQualitySolution = new CodeQualitySolution("C:\\foo", directoryHelperMock.Object, nuGetPathHelperMock.Object, fileHelperMock.Object, projectFactoryMock.Object);

            codeQualitySolution.AddOrUpdatePackage("mypackage", null, new[] { excludePattern });

            project1Mock.Verify(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>()));
            project1Mock.Verify(m => m.AddOrUpdateRuleSetReference(It.IsAny<string>()));
            project1Mock.Verify(m => m.MakePackageReferencePrivate(It.IsAny<string>()));
            project2Mock.Verify(m => m.AddOrUpdatePackage(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            project2Mock.Verify(m => m.AddOrUpdateRuleSetReference(It.IsAny<string>()), Times.Never);
            project2Mock.Verify(m => m.MakePackageReferencePrivate(It.IsAny<string>()), Times.Never);
        }
    }
}