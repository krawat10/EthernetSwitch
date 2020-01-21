using System;
using System.Diagnostics;
using EthernetSwitch.Exceptions;

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

            if(process.ExitCode > 0 || !string.IsNullOrWhiteSpace(standardError)) 
                throw new ProcessException(standardOutput, standardError, process.ExitCode);

            return standardOutput;
        }
    }
}