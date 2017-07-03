using System.IO;
using NuGet.Common;

namespace CodeQualityProfile.Client.FileSystem
{
    public class NuGetPathHelper : INuGetPathHelper
    {
        public NuGetPathHelper()
        {
            NuGetHome = NuGetEnvironment.GetFolderPath(NuGetFolderPath.NuGetHome);
        }

        public string NuGetHome { get; }

        public string GetPackageContentPath(string packageName, string version)
        {
            // NuGet stores the packages in folders with lower case names. This is important on Linux systems where the file system access is case sensitive.
            return Path.Combine(NuGetHome, "packages", packageName.ToLowerInvariant(), version, "content");
        }
    }
}