using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace CodeQualityProfile.Client.FileSystem
{
    public class XmlHelper : IXmlHelper
    {
        public XDocument LoadFromFile(string path)
        {
            return XDocument.Load(path);
        }

        public void Save(XDocument document, string path)
        {
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                var writerSettings = new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true };
                using (var writer = XmlWriter.Create(fileStream, writerSettings))
                {
                    document.Save(writer);
                }
            }
        }
    }
}