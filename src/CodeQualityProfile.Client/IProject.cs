namespace CodeQualityProfile.Client
{
    public interface IProject
    {
        string FilePath { get; }

        /// <summary>
        /// Adds or updates a package. If no version is specified, the default dotnet behavior (latest version) is used.
        /// The actually installed version is returned.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="version"></param>
        string AddOrUpdatePackage(string packageName, string version = null);

        /// <summary>
        /// Ensures that the given reference of the package is marked as private asset and no content files are consumed.
        /// Returns if the operation was successful.
        /// </summary>
        /// <param name="packageName"></param>
        /// <returns></returns>
        void MakePackageReferencePrivate(string packageName);

        /// <summary>
        /// Adds or updates the CodeAnalysisRuleSet with the given path to the *.ruleset file.
        /// </summary>
        /// <param name="relativeRuleSetPath"></param>
        /// <returns></returns>
        void AddOrUpdateRuleSetReference(string relativeRuleSetPath);
    }
}