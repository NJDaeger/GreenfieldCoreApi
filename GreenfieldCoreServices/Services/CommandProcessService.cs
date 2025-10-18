using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using GreenfieldCoreServices.Commands.Exceptions;

namespace GreenfieldCoreServices.Services;

public class CommandProcessService(ILogger<ICommandProcessService> logger, IServiceProvider serviceProvider, IHostApplicationLifetime lifetime) : ICommandProcessService, IHostedService
{
    private readonly CancellationToken _stoppingToken = lifetime.ApplicationStopping;
    private readonly ConcurrentBag<Task> _runningTasks = [];
    private Task? _commandLoopTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting command process service...");
        _commandLoopTask = CommandLoop();
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Press enter to exit...");
        if (_commandLoopTask != null)
        {
            await _commandLoopTask;
        }
    }

    /// <inheritdoc />
    public async Task ExecuteCommand(string commandLine)
    {
        var parts = commandLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            logger.LogWarning("No command provided.");
            return;
        }

        var commandName = parts[0];
        var args = parts.Skip(1).ToArray();

        using var scope = serviceProvider.CreateScope();
        var command = scope.ServiceProvider.GetKeyedService<ICommand>(commandName.ToLower());

        if (command is not null)
        {
            try
            {
                logger.LogInformation("Executing command: {Command}", commandLine);
                await command.Execute(logger, commandName, args, _stoppingToken);
                logger.LogDebug("Completed command: {Command}", commandLine);
            }
            catch (CommandExecutionException e)
            {
                logger.Log(e.LogLevel, "{Message}", e.Message);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Execution cancelled: {Command}", commandLine);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing command '{Command}'.\n\t{ErrorMessage}", commandName, ex.Message);
            }
            return;
        }

        logger.LogWarning("Command not found: {CommandName}", commandName);
    }

    /// <inheritdoc />
    public Task CommandLoop()
    {
        return Task.Run(async () =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_stoppingToken);
            logger.LogDebug("Command loop started.");
            while (!_stoppingToken.IsCancellationRequested)
            {
                string? line;
                try
                {
                    line = await Console.In.ReadLineAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                if (line is null) continue;

                // Execute command in background without awaiting
                var commandTask = Task.Run(async () =>
                {
                    try
                    {
                        await ExecuteCommand(line);
                    }
                    catch (OperationCanceledException)
                    {
                        logger.LogDebug("Command execution cancelled.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Unhandled exception executing command '{CommandLine}'", line);
                    }
                }, _stoppingToken);

                _runningTasks.Add(commandTask);
                Console.Write("> ");
            }

            logger.LogDebug("Command loop stopping. Waiting for running commands to complete...");
            
            // Wait for all running tasks to complete
            await Task.WhenAll(_runningTasks.Where(t => !t.IsCompleted));
            
            logger.LogDebug("Command loop stopped.");
        }, _stoppingToken);
    }
}