namespace CodeQualityProfile.Client.FileSystem
{
    public interface INuGetPathHelper
    {
        string NuGetHome { get; }

        string GetPackageContentPath(string packageName, string version);
    }
}