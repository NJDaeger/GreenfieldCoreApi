using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands.Exceptions;

public class CommandExecutionException : Exception
{
    public LogLevel LogLevel { get; }

    public CommandExecutionException(LogLevel logLevel = LogLevel.Warning)
    {
        LogLevel = logLevel;
    }
    
    public CommandExecutionException(string message, LogLevel logLevel = LogLevel.Warning) : base(message)
    {
        LogLevel = logLevel;
    }
}