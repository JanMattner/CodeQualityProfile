//namespace CodeQualityProfile.Client.UnitTests
//{
//    using CodeQualityProfile.Client.FileSystem;
//    using CodeQualityProfile.Client.Processes;
//    using CodeQualityProfile.Client.ViewModel;
//    using Microsoft.VisualStudio.TestTools.UnitTesting;

//    [TestClass]
//    public class UtlitiesTests
//    {
//        [TestMethod]
//        public void RegexTestForOutputVersionNullIsNotNull()
//        {
//            // Arrange
//            var utilietesViewModel = new UtilityViewModel(new DirectoryHelper(), new FileHelper(), new NuGetPathHelper());

//            var utilites = new Utilities(
//                new DirectoryHelper(),
//                new FileHelper(),
//                new XmlHelper(),
//                new CommandExecutor(@"C:\..."),
//                utilietesViewModel)
//                ;

//            const string TestString =
//                "Microsoft (R) Build Engine version 15.1.1012.6693\r\nCopyright (C) Microsoft Corporation. All rights reserved.\r\n\r\n  Writing C:\\...\r\ninfo : Adding PackageReference for package \'ait.codequalityprofile\' into project \'C:\\...\'.\r\nlog  : Restoring packages for C:\\...\r\ninfo : Package \'ait.codequalityprofile\' is compatible with all the specified frameworks in project \'C:\\...\'.\r\ninfo : PackageReference for package \'ait.codequalityprofile\' version \'0.0.1-dev20170404004\' updated in file \'C:\\...\'.\r\n";

//            // Act
//            var version = utilites.GetVersionFromStdout(TestString, "ait.codequalityprofile");

//            // Assert
//            Assert.IsNotNull(version);
//        }

//        [TestMethod]
//        public void RegexTestForOutputVersionNotNullIsNotNull()
//        {
//            // Arrange
//            var utilietesViewModel = new UtilityViewModel(new DirectoryHelper(), new FileHelper(), new NuGetPathHelper());

//            var utilites = new Utilities(
//                    new DirectoryHelper(),
//                    new FileHelper(),
//                    new XmlHelper(),
//                    new CommandExecutor(@"C:\..."),
//                    utilietesViewModel)
//                ;

//            const string TestString =
//                "Microsoft (R) Build Engine version 15.1.1012.6693\r\nCopyright (C) Microsoft Corporation. All rights reserved.\r\n\r\n  Writing C:\\...\r\ninfo : Adding PackageReference for package \'ait.codequalityprofile\' into project \'C:\\...\'.\r\nlog  : Restoring packages for C:\\...\r\ninfo : Package \'ait.codequalityprofile\' is compatible with all the specified frameworks in project \'C:\\...\'.\r\ninfo : PackageReference for package \'ait.codequalityprofile\' version \'0.0.1-dev20170404004\' updated in file \'C:\\...\'.\r\n";

//            // Act
//            var version = utilites.GetVersionFromStdout(TestString, "ait.codequalityprofile", "0.0.1-dev20170404004");

//            // Assert
//            Assert.IsNotNull(version);
//        }
//    }
//}