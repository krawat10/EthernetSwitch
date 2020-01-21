using System;

namespace EthernetSwitch.Exceptions
{
    public class ProcessException : Exception
    {
        public int ExitCode { get; }

        public ProcessException(string standardError, int exitCode)
            : base(standardError)
        {
            ExitCode = exitCode;
        }

        public ProcessException(string standardError, int exitCode, Exception inner)
            : base(standardError, inner)
        {
            ExitCode = exitCode;
        }
    }
}