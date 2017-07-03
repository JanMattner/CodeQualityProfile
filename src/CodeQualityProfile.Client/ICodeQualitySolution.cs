using System.Collections.Generic;

namespace CodeQualityProfile.Client
{
    public interface ICodeQualitySolution
    {
        void AddOrUpdatePackage(string codeQualityProfilePackageName);

        void AddOrUpdatePackage(string codeQualityProfilePackageName, string version);

        void AddOrUpdatePackage(string codeQualityProfilePackageName, string version, IReadOnlyCollection<string> exclusionPatterns);
    }
}