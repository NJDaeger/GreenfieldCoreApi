using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Commands;

public class TestCommand : BaseCommand
{
    public override async Task Execute(ILogger<ICommandProcessService> logger, string alias, string[] args,
        CancellationToken cancellationToken)
    {
        //return a task that says 1, 2, 3, 4, 5 with a delay of 1 second between each number
        for (var i = 1; i <= 5; i++) 
        {
            Console.WriteLine(i);
            await Task.Delay(1000, cancellationToken);
        }
    }
}