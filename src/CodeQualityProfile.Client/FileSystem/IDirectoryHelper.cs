using System.IO;

namespace CodeQualityProfile.Client.FileSystem
{
    public interface IDirectoryHelper
    {
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories, string workingDirectory = ".");
    }
}