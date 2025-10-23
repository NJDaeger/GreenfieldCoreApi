using GreenfieldCoreDataAccess.Database.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GreenfieldCoreDataAccess.Database.ScriptManager;

public abstract class BaseScriptManager : IScriptManager
{
    private readonly ILogger<IScriptManager> _logger;
    internal readonly IUnitOfWork UnitOfWork;
    internal readonly IConfiguration Configuration;
    internal readonly string ScriptsRoot;
    
    protected BaseScriptManager(ILogger<IScriptManager> logger, IConfiguration config, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        UnitOfWork = unitOfWork;
        Configuration = config;
        var scriptRoot = config.GetValue<string>("ScriptsRoot") ?? throw new ArgumentException("ScriptsRoot not configured.");
        ScriptsRoot = Path.Combine(Directory.GetParent(Environment.CurrentDirectory)?.FullName, scriptRoot);
        
        _logger.LogInformation("Scripts root directory: {ScriptsRoot}", ScriptsRoot);
        _logger.LogInformation("Running script manager...");
    }
    
    public async Task ApplyPendingScripts(CancellationToken cancellationToken)
    {
        var scriptBeingApplied = "";
        if (!Directory.Exists(ScriptsRoot)) return;

        try
        {
            var scriptObjects = GetScripts(ScriptsRoot).ToList();

            // Group by AppliesTo and order within each group
            var groupedScripts = scriptObjects
                .GroupBy(s => s.AppliesTo, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => OrderScripts(g.ToList()).ToList(), // keep deterministic order
                    StringComparer.OrdinalIgnoreCase);

            var processed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var visiting = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            
            UnitOfWork.BeginTransaction();
            
            foreach (var script in scriptObjects)
            {
                if (cancellationToken.IsCancellationRequested) break;
                scriptBeingApplied = script.FilePath;
                await RunScriptAsync(script, groupedScripts, processed, visiting, cancellationToken);
                var newPercentage = (int)((processed.Count / (double)scriptObjects.Count) * 100);
                _logger.LogInformation("Script application progress: {Percentage:F2}%", newPercentage);
            }
        }
        catch (Exception ex)
        {
            UnitOfWork.Rollback();
            _logger.LogError(ex, "Error applying scripts... Last script attempted: {Script}", scriptBeingApplied);
            throw;
        }
        finally
        {
            UnitOfWork.CompleteAndCommit();
            _logger.LogInformation("Script application complete.");
        }
    }

    private async Task RunScriptAsync(
        Script script,
        IReadOnlyDictionary<string, List<Script>> groupedScripts,
        HashSet<string> processed,
        HashSet<string> visiting,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested) return;

        // Skip if already processed (executed or skipped)
        if (processed.Contains(script.FilePath)) return;

        // Detect circular dependency at file-level
        if (!visiting.Add(script.FilePath))
        {
            _logger.LogWarning("Detected circular dependency involving: {File}", script.FilePath);
            return;
        }

        try
        {
            // Ensure dependencies (by AppliesTo) are processed first
            if (script.DependsOn is { Count: > 0 })
            {
                foreach (var dependency in script.DependsOn)
                {
                    if (cancellationToken.IsCancellationRequested) return;

                    if (!groupedScripts.TryGetValue(dependency, out var depScripts) || depScripts.Count == 0)
                    {
                        _logger.LogDebug("No scripts found for dependency '{Dependency}' required by {Script}", dependency, script.FilePath);
                        continue;
                    }

                    foreach (var depScript in depScripts)
                    {
                        await RunScriptAsync(depScript, groupedScripts, processed, visiting, cancellationToken);
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested) return;

            // Decide whether to execute this script
            if (await ShouldScriptExecute(script))
            {
                var scriptText = await File.ReadAllTextAsync(script.FilePath, cancellationToken);
                _logger.LogInformation("Applying script: {ScriptName}", script.FilePath);
                await ExecuteScript(scriptText);
                await RecordScriptExecution(script);
            }
            else
            {
                _logger.LogDebug("Skipping script (no-op): {ScriptName}", script.FilePath);
            }

            // Mark as processed regardless of execution result
            processed.Add(script.FilePath);
        }
        finally
        {
            // Always remove from visiting to avoid leaking state on early returns
            visiting.Remove(script.FilePath);
        }
    }
    
    private IEnumerable<Script> GetScripts(string root)
    {
        return Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories).Select(Script.FromFile);
    }
    
    private IEnumerable<Script> OrderScripts(IEnumerable<Script> scripts)
    {
        return scripts
            .OrderBy(s => s.FilePath.Contains("ScriptHistory", StringComparison.OrdinalIgnoreCase) ? 0 : 1) // ScriptHistory first
            .ThenBy(s => s.AppliesTo)
            .ThenBy(s => s.IsSproc ? 1 : 0) // Non-sproc first
            .ThenBy(s => s.IsInit ? 0 : 1) // Init
            .ThenBy(s => s.Major)
            .ThenBy(s => s.Minor);
    }
    
    public IEnumerable<Script> EnumerateScripts(string root)
    {
        var files = Directory.GetFiles(root, "*.sql", SearchOption.AllDirectories);

        return files
            .Select(Script.FromFile)
            .OrderBy(s => s.FilePath.Contains("ScriptHistory", StringComparison.OrdinalIgnoreCase) ? 0 : 1) // ScriptHistory first
            .ThenBy(s => s.AppliesTo)
            .ThenBy(s => s.IsSproc ? 1 : 0) // Non-sproc first
            .ThenBy(s => s.IsInit ? 0 : 1) // Init
            .ThenBy(s => s.Major)
            .ThenBy(s => s.Minor);
    }

    public abstract Task<bool> HasBeenInitialized();

    public abstract Task<bool> ShouldScriptExecute(Script script);
    
    public abstract Task RecordScriptExecution(Script script);

    public abstract Task ExecuteScript(string script);
}
