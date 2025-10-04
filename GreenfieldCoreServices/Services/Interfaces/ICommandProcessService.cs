namespace GreenfieldCoreServices.Services.Interfaces;

public interface ICommandProcessService
{
    
    /// <summary>
    /// Executes a command line string.
    /// </summary>
    /// <param name="commandLine"></param>
    /// <returns></returns>
    public Task ExecuteCommand(string commandLine);

    /// <summary>
    /// Starts a command loop that reads commands from the console.
    /// </summary>
    /// <returns></returns>
    public Task CommandLoop();

}