using System.Xml.Linq;

namespace CodeQualityProfile.Client.FileSystem
{
    public interface IXmlHelper
    {
        /// <summary>Copies an existing file to a new file. Overwriting a file of the same name is allowed.</summary>
        /// <param name="path"></param>
        XDocument LoadFromFile(string path);

        /// <summary>Saves the xml document to the specific path</summary>
        /// <param name="document"></param>
        /// <param name="path"></param>
        void Save(XDocument document, string path);
    }
}