//namespace CodeQualityProfile.Client.UnitTests
//{
//    using System.IO;
//    using System.Linq;
//    using System.Xml.Linq;
//    using CodeQualityProfile.Client.FileSystem;
//    using Microsoft.VisualStudio.TestTools.UnitTesting;

//    [TestClass]
//    public class XmlHelperTests
//    {
//        [TestMethod]
//        public void AddPrivateAssetsNodeXdocHaveNewNode()
//        {
//            // Arrange
//            var xmlHelper = new XmlHelper();

//            // Act
//            XElement privateAssests = null;

//            var xDoc = GetTesDocument();

//            var newXdoc = xmlHelper.AddPrivateAssetsNode(xDoc, "AIT.codequalityprofile");
//            var xElement = newXdoc.Element("Project");
//            if (xElement != null)
//            {
//                foreach (var node in xElement.Elements("ItemGroup"))
//                {
//                    foreach (var subNode in node.Elements())
//                    {
//                        if (subNode.Attributes().Any(x => x.Value.ToLowerInvariant() == "AIT.codequalityprofile".ToLowerInvariant()))
//                        {
//                            privateAssests = subNode;
//                        }
//                    }
//                }
//            }

//            // Assert
//            Assert.IsNotNull(privateAssests != null && privateAssests.Elements().All(attribute => attribute.Value == "All"));
//        }

//        [TestMethod]
//        public void AddRuleSetNodeNodeXdocHaveNewNode()
//        {
//            // Arrange
//            var xmlHelper = new XmlHelper();

//            // Act
//            var xDoc = GetTesDocument();
//            var newXdoc = xmlHelper.AddRuleSetNode(xDoc, "CodeQualityProfile.ruleset", 0);

//            XElement codeAnalysisRuleSet = null;

//            var xElement = newXdoc.Element("Project");
//            if (xElement != null)
//            {
//                codeAnalysisRuleSet = xElement.Elements("PropertyGroup").First(element => element.Elements().Any(attritube => attritube.Name == "TargetFramework" || attritube.Name == "TargetFrameworkVersion"));
//            }

//            // Assert
//            Assert.IsNotNull(codeAnalysisRuleSet != null && codeAnalysisRuleSet.Elements().Any(element => element.Name == "CodeAnalysisRuleSet"));
//        }

//        private static XDocument GetTesDocument()
//        {
//            TextReader tr = new StringReader(
//                "<Project Sdk=\"Microsoft.NET.Sdk\">\r\n  <PropertyGroup>\r\n    <OutputType>Exe</OutputType>\r\n    <TargetFramework>netcoreapp1.1</TargetFramework>\r\n  </PropertyGroup>\r\n  <ItemGroup>\r\n    <PackageReference Include=\"AIT.CodeQualityProfile\" Version=\"0.0.1-dev20170404004\">\r\n    </PackageReference>\r\n  </ItemGroup>\r\n</Project>");
//                var doc = XDocument.Load(tr);

//            return doc;
//        }
//    }
//}