namespace CodeQualityProfile.Client.Processes
{
    public interface ICommandExecutor
    {
        CommandResult ExecuteCommand(string program, string arguments, string workingDirectory = null);
    }

    public struct CommandResult
    {
        public int ExitCode;

        public string StdOut;
    }
}