namespace CodeQualityProfile.Client
{
    public interface IProjectFactory
    {
        IProject CreateProject(string filePath);
    }
}