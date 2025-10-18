using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Interfaces;

public interface ICommand
{

    /// <summary>
    /// Gets the description of the command.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Gets the usage string for the command.
    /// </summary>
    string Usage { get; }
    
    /// <summary>
    /// Executes the command with the given alias and arguments.
    /// </summary>
    /// <param name="logger">The command process logger</param>
    /// <param name="alias">The name of the command used</param>
    /// <param name="args">The arguments (does not include the alias), or an empty array if there were no arguments.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the command execution.</param>
    /// <returns>A command task.</returns>
    Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args,
        CancellationToken cancellationToken);
    
}