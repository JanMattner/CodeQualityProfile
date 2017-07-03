using System.IO;

namespace CodeQualityProfile.Client.FileSystem
{
    public class DirectoryHelper : IDirectoryHelper
    {
        public string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories, string workingDirectory = ".")
        {
            var savedDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(workingDirectory);
            var files = Directory.GetFiles(path, searchPattern, searchOption);
            Directory.SetCurrentDirectory(savedDir);
            return files;
        }
    }
}