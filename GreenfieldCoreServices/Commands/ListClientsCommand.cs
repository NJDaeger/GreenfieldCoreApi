using GreenfieldCoreServices.Commands.Exceptions;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public class ListClientsCommand(IClientAuthService clientAuthService) : BaseCommand("List all registered clients", "list-clients")
{
    public override async Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args, CancellationToken cancellationToken)
    {
        var clients = (await clientAuthService.GetAllClients()).ToList();
        
        if (clients.Count == 0) 
            throw new CommandExecutionException("There are no registered clients.");
        
        foreach (var client in clients)
        {
            logger.LogInformation("Client Name: {ClientName}, Client ID: {ClientId}, Created On: {CreatedOn}", client.ClientName, client.ClientId, client.CreatedOn); 
        }
    }
}