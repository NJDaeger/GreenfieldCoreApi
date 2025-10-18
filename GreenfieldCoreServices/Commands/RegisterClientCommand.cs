using GreenfieldCoreServices.Commands.Exceptions;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public class RegisterClientCommand(IClientAuthService authService) : BaseCommand("Register a new client", "register-client <clientName> [roles...]")
{
    public override async Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args, CancellationToken cancellationToken)
    {
        var clientName = args.GetArg<string>(0);
        if (clientName is null)
            throw new CommandExecutionException("Client name is required. Usage: " + Usage);

        var roles = args.Skip(1).ToList();
        
        var clientTuple = await authService.RegisterClient(clientName, roles);
        
        logger.LogInformation("Client registered successfully. Copy these details, the secret will not be shown again:\n  Client ID: {ClientId}\n  Client Secret: {ClientSecret}", clientTuple.client.ClientId, clientTuple.secret);
    }
}