using System.Xml.Linq;

namespace CodeQualityProfile.Client.FileSystem
{
    public interface IXmlHelper
    {
        XDocument LoadFromFile(string path);

        void Save(XDocument document, string path);
    }
}