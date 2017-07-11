using System.Collections.Generic;

namespace CodeQualityProfile.Client
{
    public interface ICodeQualitySolution
    {
        void AddOrUpdatePackage(string codeQualityProfilePackageName, string version = null, IReadOnlyCollection<string> exclusionPatterns = null);
    }
}