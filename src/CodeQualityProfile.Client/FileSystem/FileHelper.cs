using System.IO;

namespace CodeQualityProfile.Client.FileSystem
{
    public class FileHelper : IFileHelper
    {
        public void Copy(string source, string destination, bool overwrite)
        {
            File.Copy(source, destination, overwrite);
        }
    }
}