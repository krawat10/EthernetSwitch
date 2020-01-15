using System;
using System.Diagnostics;

namespace EthernetSwitch.Infrastructure
{
    class BashCommand : IBashCommand
    {
        public string Execute(string command)
        {
            var escapedArgs = command.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true
                }
            };
            process.Start();
            
            var standardOutput = process.StandardOutput.ReadToEnd();
            var standardError = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if(process.ExitCode > 0) throw new ApplicationException($"Cannot execute command, error: {standardError}");

            return standardOutput;
        }
    }
}