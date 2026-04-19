using Dapper;
using GreenfieldCoreDataAccess.Database.Procedures;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.ScriptManager;

public class ScriptManager(ILogger<IScriptManager> logger, IConfiguration config, IUnitOfWork unitOfWork) : BaseScriptManager(logger, config, unitOfWork)
{
    
    private bool? _hasBeenInitialized;
    
    public override async Task<bool> HasBeenInitialized()
    { 
        if (_hasBeenInitialized.HasValue && _hasBeenInitialized.Value) return _hasBeenInitialized.Value;
        
        var tableExists = await UnitOfWork.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ScriptManager.ScriptHistory';",
            transaction: UnitOfWork.Transaction);
        
        _hasBeenInitialized = tableExists > 0;
        return _hasBeenInitialized.Value;
    }

    public override async Task<bool> ShouldScriptExecute(Script script)
    {
        if (!await HasBeenInitialized()) return script.IsInit && script.FilePath.Contains("ScriptHistory");
        return await UnitOfWork.Connection.ExecuteScalarProcedure(StoredProcs.ScriptManager.ShouldScriptBeApplied, script, UnitOfWork.Transaction);
    }

    public override Task RecordScriptExecution(Script script)
    {
        return UnitOfWork.Connection.ExecuteProcedure(StoredProcs.ScriptManager.RecordScriptExecution, script, UnitOfWork.Transaction);
    }

    public override Task ExecuteScript(string script)
    {
        return UnitOfWork.Connection.ExecuteAsync(script, transaction: UnitOfWork.Transaction);
    }
}