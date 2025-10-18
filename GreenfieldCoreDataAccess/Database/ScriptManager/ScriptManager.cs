using System.Data;
using Dapper;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreDataAccess.Database.ScriptManager;

public class ScriptManager(ILogger<IScriptManager> logger, IConfiguration config, IUnitOfWork unitOfWork) : BaseScriptManager(logger, config, unitOfWork)
{
    
    private const string RecordScriptProc = "usp_RecordScriptExecution";
    private const string CheckScriptProc = "usp_ShouldScriptBeApplied";
    
    public override async Task<bool> HasBeenInitialized()
    { 
        var tableExists = await UnitOfWork.Connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'ScriptHistory';",
            transaction: UnitOfWork.Transaction);
        
        return tableExists > 0;
    }

    public override async Task<bool> ShouldScriptExecute(Script script)
    {
        if (!await HasBeenInitialized()) return script.IsInit && script.FilePath.Contains("ScriptHistory");
        
        var parameters = new DynamicParameters();
        parameters.Add("p_IsInit", script.IsInit, DbType.Boolean);
        parameters.Add("p_AppliesTo", script.AppliesTo, DbType.String, size: 255);
        parameters.Add("p_Major", script.Major, DbType.Int32);
        parameters.Add("p_Minor", script.Minor, DbType.Int32);
        
        return await UnitOfWork.Connection.ExecuteScalarAsync<bool>(CheckScriptProc, parameters, commandType: CommandType.StoredProcedure, transaction: UnitOfWork.Transaction);
        
    }

    public override Task RecordScriptExecution(Script script)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_IsInit", script.IsInit, DbType.Boolean);
        parameters.Add("p_AppliesTo", script.AppliesTo, DbType.String, size: 255);
        parameters.Add("p_Major", script.Major, DbType.Int32);
        parameters.Add("p_Minor", script.Minor, DbType.Int32);
        
        return UnitOfWork.Connection.ExecuteAsync(RecordScriptProc, parameters, commandType: CommandType.StoredProcedure, transaction: UnitOfWork.Transaction);
    }

    public override Task ExecuteScript(string script)
    {
        return UnitOfWork.Connection.ExecuteAsync(script, transaction: UnitOfWork.Transaction);
    }
}