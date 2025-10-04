using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public abstract class BaseCommand : ICommand
{

    /// <inheritdoc />
    public abstract Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args,
        CancellationToken cancellationToken);
}