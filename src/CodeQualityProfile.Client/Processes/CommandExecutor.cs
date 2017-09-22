using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace CodeQualityProfile.Client.Processes
{
    public class CommandExecutor : ICommandExecutor
    {
        private readonly string _workingDirectory;

        private readonly ILogger _logger = ApplicationLogging.CreateLogger<CommandExecutor>();

        public CommandExecutor(string workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory))
            {
                throw new ArgumentNullException(nameof(workingDirectory));
            }

            _workingDirectory = workingDirectory;
        }

        public CommandResult ExecuteCommand(string program, string arguments, string workingDirectory = null)
        {
            if (string.IsNullOrWhiteSpace(program))
            {
                throw new ArgumentNullException(nameof(program));
            }

            using (var process = new Process())
            {
                _logger.LogTrace($"Execute: {program} {arguments}");

                var output = string.Empty;

                process.StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    WorkingDirectory = workingDirectory ?? _workingDirectory
                };

                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        _logger.LogDebug(eventArgs.Data);
                        output += "\n" + eventArgs.Data;
                    }
                };
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        _logger.LogError(eventArgs.Data);
                    }
                };
                process.Start();
                process.BeginErrorReadLine();
                process.BeginOutputReadLine();
                process.WaitForExit();

                return new CommandResult { ExitCode = process.ExitCode, StdOut = output };
            }
        }
    }
}