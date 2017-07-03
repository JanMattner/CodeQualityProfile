using CodeQualityProfile.Client.FileSystem;
using CodeQualityProfile.Client.Processes;

namespace CodeQualityProfile.Client
{
    public class ProjectFactory : IProjectFactory
    {
        private readonly ICommandExecutor _commandExecutor;

        private readonly IXmlHelper _xmlHelper;

        public ProjectFactory(ICommandExecutor commandExecutor, IXmlHelper xmlHelper)
        {
            _commandExecutor = commandExecutor;
            _xmlHelper = xmlHelper;
        }

        public IProject CreateProject(string filePath)
        {
            return new Project(filePath, _commandExecutor, _xmlHelper);
        }
    }
}