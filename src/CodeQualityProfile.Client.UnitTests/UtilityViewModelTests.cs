//namespace CodeQualityProfile.Client.UnitTests
//{
//    using System.Collections.Generic;
//    using System.IO;
//    using CodeQualityProfile.Client.FileSystem;
//    using CodeQualityProfile.Client.ViewModel;
//    using Microsoft.VisualStudio.TestTools.UnitTesting;
//    using Moq;

//    [TestClass]
//    public class UtilityViewModelTests
//    {
//        [TestMethod]
//        public void SetInformationNotNull()
//        {
//            // Arrange
//            var directoryHelperMock = new Mock<IDirectoryHelper>();

//            string[] files = { "cproj1", "cproj2", "cproj3", "cproj4" };

//            directoryHelperMock.Setup(gfs => gfs.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns(files);
//            directoryHelperMock.Setup(gf => gf.GetFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>())).Returns("file");
//            directoryHelperMock.Setup(gfn => gfn.GetFileName(It.IsAny<string>())).Returns("fileName");
//            directoryHelperMock.Setup(gpd => gpd.GetPathDepht(It.IsAny<string>(), It.IsAny<string>())).Returns(1);

//            var fileHelperMock = new Mock<IFileHelper>();

//            fileHelperMock.Setup(c => c.Copy(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), true));

//            var nugetPathHelperMock = new Mock<INuGetPathHelper>();

//            nugetPathHelperMock.Setup(nh => nh.NuGetHome).Returns("NugetHome");
//            nugetPathHelperMock.Setup(bnhp => bnhp.BuildNugetHomePath(It.IsAny<string>(), It.IsAny<string>()));

//            var utilityViewModel = new UtilityViewModel(directoryHelperMock.Object, fileHelperMock.Object, nugetPathHelperMock.Object);

//            // Act
//            utilityViewModel.SetSolutionInformation("startPath");
//            utilityViewModel.VersionNumber = new List<string> { "version1", "version2" };
//            utilityViewModel.SetFileInformation("packageName");

//            // Assert
//            Assert.IsNotNull(utilityViewModel.ProjectFiles);
//            Assert.IsNotNull(utilityViewModel.VersionNumber);
//            Assert.IsNotNull(utilityViewModel.DotNetSettingsFilePath);
//            Assert.IsNotNull(utilityViewModel.RuleSetFileName);
//            Assert.IsNotNull(utilityViewModel.SolutionFilePath);
//            Assert.IsNotNull(utilityViewModel.SolutionName);
//        }
//    }
//}