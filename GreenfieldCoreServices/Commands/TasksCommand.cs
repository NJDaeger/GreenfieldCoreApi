using GreenfieldCoreServices.Commands.Exceptions;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public class TasksCommand(IServiceProvider serviceProvider) : BaseCommand("Manually trigger background tasks.", "tasks <help|refresh> [task-name]")
{
    public override Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args, CancellationToken cancellationToken)
    {
        var subCommand = args.GetArg<string>(0)?.ToLower() ?? "help";

        if (subCommand == "refresh") Refresh(logger, args.Skip(1).ToArray(), cancellationToken);
        else ShowHelp(logger);

        return Task.CompletedTask;
    }

    private void ShowHelp(ILogger<ICommandProcessService> logger)
    {
        logger.LogInformation("""
                              Tasks Command Subcommands:
                                help - Show this help message.
                                refresh <task-name> - Trigger a token refresh task immediately.
                                  Available tasks: discord, patreon
                              """);
    }

    private void Refresh(ILogger<ICommandProcessService> logger, string[] args, CancellationToken cancellationToken)
    {
        var taskName = args.GetArg<string>(0)?.ToLower();
        if (taskName is null)
            throw new CommandExecutionException("Task name is required. Usage: " + Usage);

        var trigger = serviceProvider.GetKeyedService<ITokenRefreshTrigger>(taskName);
        if (trigger is null)
            throw new CommandExecutionException($"Unknown task '{taskName}'. Available tasks: discord, patreon");

        _ = trigger.TriggerAsync(cancellationToken);
        logger.LogInformation("Refresh task for '{TaskName}' has been triggered.", taskName);
    }
}

