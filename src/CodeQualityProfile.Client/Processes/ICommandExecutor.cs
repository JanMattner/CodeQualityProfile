namespace CodeQualityProfile.Client.Processes
{
    public interface ICommandExecutor
    {
        /// <summary>
        /// Executes a command with attributes in a specified path
        /// </summary>
        /// <param name="program"></param>
        /// <param name="arguments"></param>
        /// <param name="workingDirectory"></param>
        /// <returns>A struct with the exit code and command output</returns>
        CommandResult ExecuteCommand(string program, string arguments, string workingDirectory = null);
    }

    /// <summary>Represents the result of a command execution.</summary>
    public struct CommandResult
    {
        public int ExitCode;

        public string StdOut;
    }
}