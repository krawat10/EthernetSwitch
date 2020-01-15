using System.Diagnostics;

namespace EthernetSwitch.Infrastructure
{
    public interface IBashCommand
    {
        (string, string) Execute(string command);
    }

    class BashCommand : IBashCommand
    {
        public (string, string) Execute(string command)
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
                }
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            
            process.WaitForExit();
            return (string.Empty, string.Empty);
        }
    }
}