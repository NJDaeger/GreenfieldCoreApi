using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public abstract class BaseCommand(string description, string usage) : ICommand
{
    /// <inheritdoc />
    public string Description { get; } = description;

    /// <inheritdoc />
    public string Usage { get; } = usage;

    /// <inheritdoc />
    public abstract Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args,
        CancellationToken cancellationToken);
}