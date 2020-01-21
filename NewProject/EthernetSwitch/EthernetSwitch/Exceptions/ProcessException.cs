using System;

namespace EthernetSwitch.Exceptions
{
    public class ProcessException : Exception
    {
        public string StandardOutput { get; }
        public int ExitCode { get; }

        public ProcessException(string standardOutput, string standardError, int exitCode)
            : base(standardError)
        {
            StandardOutput = standardOutput;
            ExitCode = exitCode;
        }

        public ProcessException(string standardOutput, string standardError, int exitCode, Exception inner)
            : base(standardError, inner)
        {
            StandardOutput = standardOutput;
            ExitCode = exitCode;
        }

        public ProcessException(string standardError, int exitCode)
            : base(standardError)
        {
            ExitCode = exitCode;
        }

        public ProcessException( string standardError, int exitCode, Exception inner)
            : base(standardError, inner)
        {
            ExitCode = exitCode;
        }
    }
}