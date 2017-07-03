using System;
using System.Linq;
using System.Xml.Linq;
using CodeQualityProfile.Client.FileSystem;
using CodeQualityProfile.Client.Processes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace CodeQualityProfile.Client.UnitTests
{
    [TestClass]
    public class ProjectTests
    {
        [TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Project_ConstructorWithRelativePath_ShouldThrowException()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();
            var project = new Project(".\\relative\\path", commandMock.Object, xmlMock.Object);
        }

        [TestMethod]
        public void Project_AddOrUpdatePackage_ShouldReturnInstalledVersion()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();

            commandMock.Setup(m => m.ExecuteCommand("dotnet", It.IsRegex("^add .+ somepackage -v 1.2.3$"), It.IsAny<string>()))
                .Returns(new CommandResult() { ExitCode = 0, StdOut = "PackageReference for package 'somepackage' version '1.2.3' added in file" });

            var project = new Project("C:\\foo\\bar.csproj", commandMock.Object, xmlMock.Object);

            var version = project.AddOrUpdatePackage("somepackage", "1.2.3");

            Assert.AreEqual("1.2.3", version);
        }

        [TestMethod]
        public void Project_AddOrUpdatePackageWithDifferentVersion_ShouldReturnInstalledVersion()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();

            commandMock.Setup(m => m.ExecuteCommand("dotnet", It.IsRegex("^add .+ somepackage -v 4.5.6$"), It.IsAny<string>()))
                .Returns(new CommandResult() { ExitCode = 0, StdOut = "PackageReference for package 'somepackage' version '1.2.3' added in file" });

            var project = new Project("C:\\foo\\bar.csproj", commandMock.Object, xmlMock.Object);

            var version = project.AddOrUpdatePackage("somepackage", "4.5.6");

            Assert.AreEqual("1.2.3", version);
        }

        [TestMethod]
        public void Project_AddOrUpdatePackageWithNoVersion_ShouldReturnInstalledVersion()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();

            commandMock.Setup(m => m.ExecuteCommand("dotnet", It.IsRegex("^add .+ somepackage$"), It.IsAny<string>()))
                .Returns(new CommandResult() { ExitCode = 0, StdOut = "PackageReference for package 'somepackage' version '1.2.3' added in file" });

            var project = new Project("C:\\foo\\bar.csproj", commandMock.Object, xmlMock.Object);

            var version = project.AddOrUpdatePackage("somepackage", null);

            Assert.AreEqual("1.2.3", version);
        }

        [TestMethod]
        public void Project_MakePackageReferencePrivate_PrivateAndExcludedAssetsAdded()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();

            var xdoc = new XDocument(new XElement("Project", new XElement("ItemGroup", new XElement("PackageReference", new XAttribute("Include", "Some.Package")))));

            xmlMock.Setup(m => m.LoadFromFile(It.IsAny<string>())).Returns(xdoc);

            var project = new Project("C:\\foo\\bar.csproj", commandMock.Object, xmlMock.Object);

            project.MakePackageReferencePrivate("Some.Package");

            xmlMock.Verify(m => m.Save(xdoc, It.IsAny<string>()));

            Assert.AreEqual(1, xdoc.Elements("Project").Count());
            Assert.AreEqual(1, xdoc.Element("Project")?.Elements("ItemGroup").Count());
            Assert.AreEqual(1, xdoc.Element("Project")?.Element("ItemGroup")?.Elements("PackageReference").Count());
            var referenceElement = xdoc.Element("Project")?.Element("ItemGroup")?.Element("PackageReference");
            Assert.IsNotNull(referenceElement);
            Assert.AreEqual("Some.Package", referenceElement.Attribute("Include")?.Value);
            Assert.AreEqual(1, referenceElement.Elements("PrivateAssets").Count());
            Assert.AreEqual("All", referenceElement.Element("PrivateAssets")?.Value);
            Assert.AreEqual(1, referenceElement.Elements("ExcludeAssets").Count());
            Assert.AreEqual("contentFiles", referenceElement.Element("ExcludeAssets")?.Value);
        }

        [TestMethod]
        public void Project_MakePackageReferencePrivateWithMultipleItemGroups_PrivateAndExcludedAssetsAdded()
        {
            var commandMock = new Mock<ICommandExecutor>();
            var xmlMock = new Mock<IXmlHelper>();

            var xdoc = new XDocument(new XElement(
                "Project",
                new XElement("ItemGroup", new XElement("PackageReference", new XAttribute("Include", "Some.Package"))),
                new XElement("ItemGroup", new XElement("ProjectReference", "hello world")),
                new XElement("ItemGroup", new XElement("Service", "foo"))));

            xmlMock.Setup(m => m.LoadFromFile(It.IsAny<string>())).Returns(xdoc);

            var project = new Project("C:\\foo\\bar.csproj", commandMock.Object, xmlMock.Object);

            project.MakePackageReferencePrivate("Some.Package");

            xmlMock.Verify(m => m.Save(xdoc, It.IsAny<string>()));

            Assert.AreEqual(1, xdoc.Elements("Project").Count());
            var projectElement = xdoc.Elements("Project");
            Assert.IsNotNull(projectElement);
            var itemGroups = projectElement.Elements("ItemGroup").ToList();
            Assert.AreEqual(3, itemGroups.Count);
            var group = itemGroups.FirstOrDefault(e => e.Elements().Count() == 1 && e.Element("Service") != null);
            Assert.IsNotNull(group);
            var elem = group.Element("Service");
            Assert.IsNotNull(elem);
            Assert.AreEqual("foo", elem.Value);
            Assert.IsFalse(elem.HasElements);
            group = itemGroups.FirstOrDefault(e => e.Elements().Count() == 1 && e.Element("ProjectReference") != null);
            Assert.IsNotNull(group);
            elem = group.Element("ProjectReference");
            Assert.IsNotNull(elem);
            Assert.AreEqual("hello world", elem.Value);
            Assert.IsFalse(elem.HasElements);

            group = itemGroups.FirstOrDefault(e => e.Elements().Count() == 1 && e.Element("PackageReference") != null);
            Assert.IsNotNull(group);
            var referenceElement = group.Element("PackageReference");
            Assert.IsNotNull(referenceElement);
            Assert.AreEqual("Some.Package", referenceElement.Attribute("Include")?.Value);
            Assert.AreEqual(1, referenceElement.Elements("PrivateAssets").Count());
            Assert.AreEqual("All", referenceElement.Element("PrivateAssets")?.Value);
            Assert.AreEqual(1, referenceElement.Elements("ExcludeAssets").Count());
            Assert.AreEqual("contentFiles", referenceElement.Element("ExcludeAssets")?.Value);
        }
    }
}