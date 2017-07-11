using System.IO;

namespace CodeQualityProfile.Client.FileSystem
{

    public interface IDirectoryHelper
    {
        /// <summary>
        /// Searches for files with specified search options and patterns in an appropriate path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="searchPattern"></param>
        /// <param name="searchOption"></param>
        /// <param name="workingDirectory"></param>
        /// <returns></returns>
        string[] GetFiles(string path, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories, string workingDirectory = ".");
    }
}