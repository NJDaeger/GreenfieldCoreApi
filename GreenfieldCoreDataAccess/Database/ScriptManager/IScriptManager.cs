using Microsoft.Data.SqlClient;

namespace GreenfieldCoreDataAccess.Database.ScriptManager;

public interface IScriptManager
{
    Task ApplyPendingScripts(CancellationToken cancellationToken);
    
    IEnumerable<Script> EnumerateScripts(string root);

    Task<bool> HasBeenInitialized();

    Task<bool> ShouldScriptExecute(Script script);
    
    Task RecordScriptExecution(Script script);
    
    Task ExecuteScript(string script);
}