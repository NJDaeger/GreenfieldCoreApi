using GreenfieldCoreServices.Models.Connections.Patreon;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Tasks;

public class PatreonTokenRefreshTask(TaskStartSignalService startSignal, IServiceScopeFactory scopeFactory, ILogger<PatreonTokenRefreshTask> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startSignal.WaitForStartAsync(stoppingToken);
        
        await RefreshPatreonTokensAsync(stoppingToken);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                await RefreshPatreonTokensAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, exit gracefully
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Patreon token refresh task.");
            }
        }
        
    }
    
    private async Task RefreshPatreonTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var patreonService = scope.ServiceProvider.GetRequiredService<IPatreonService>();
        var patreonApi = scope.ServiceProvider.GetRequiredService<IPatreonApi>();
        var campaignIdResult = await patreonApi.ResolveCampaignId();

        if (!campaignIdResult.TryGetDataNonNull(out var campaignId))
        {
            logger.LogError("Failed to resolve Patreon campaign ID. **This will stop all refresh processes for Patreon tokens!** Error: {ErrorMessage}", campaignIdResult.ErrorMessage);
            return;
        }

        logger.LogInformation("Starting Patreon token refresh task at {Time}", DateTimeOffset.Now);
        var allAccountsResult = await patreonService.GetAllPatreonConnections();

        if (!allAccountsResult.TryGetDataNonNull(out var connections))
        {
            logger.LogWarning("Failed to retrieve Patreon accounts for token refresh. Error: {ErrorMessage}", allAccountsResult.ErrorMessage);
            return;
        }
        
        var semaphore = new SemaphoreSlim(4);
        var refreshTasks = new List<Task>();
        var totalRefreshed = 0;

        foreach (var connection in connections)
        {
            await semaphore.WaitAsync(cancellationToken);
            refreshTasks.Add(Task.Run(async () =>
            {
                using var semScope = scopeFactory.CreateScope();
                var semPatreonService = semScope.ServiceProvider.GetRequiredService<IPatreonService>();
                var semPatreonApi = semScope.ServiceProvider.GetRequiredService<IPatreonApi>();
                try
                {
                    if (await RefreshConnection(connection, semPatreonApi, semPatreonService, campaignId)) 
                        Interlocked.Increment(ref totalRefreshed);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(refreshTasks);
        logger.LogInformation("Completed Patreon token refresh task at {Time}. Refreshed {Count} tokens.", DateTimeOffset.Now, totalRefreshed);
    }

    private async Task<bool> RefreshConnection(PatreonConnection connection, IPatreonApi patreonApi, IPatreonService patreonService, string campaignId)
    {
        var expiresIn = connection.RefreshBy - DateTime.Now;
        if (connection.RefreshBy <= DateTime.Now)
        {
            logger.LogWarning("RefreshTask {PatreonConnectionId}: Patreon token expired. All linked users will be unlinked.", connection.PatreonConnectionId);
            await patreonService.DeletePatreonConnection(connection.PatreonConnectionId);
            return false;
        }

        if (expiresIn > TimeSpan.FromDays(2))
            return false;
        
        var refreshResult = await patreonApi.RefreshPatreonAccessTokenAsync(connection.RefreshToken);
        if (!refreshResult.TryGetDataNonNull(out var tokenResponse))
        {
            logger.LogError("RefreshTask {PatreonConnectionId}: Failed to refresh Patreon token. Error: {ErrorMessage}", 
                connection.PatreonConnectionId, refreshResult.ErrorMessage);
            return false;
        }
        
        var latestFullName = connection.FullName;
        var latestPledge = connection.Pledge;
        
        var identityResult = await patreonApi.GetPatreonIdentity(tokenResponse.AccessToken);
        if (identityResult.TryGetDataNonNull(out var identity))
        {
            latestFullName = identity.Data.Attributes?.FullName ?? latestFullName;
            latestPledge = identity.GetPledgedAmountOfCampaign(campaignId);
        } 
        else
            logger.LogWarning("RefreshTask {PatreonConnectionId}: Failed to fetch Patreon identity. Their non-token information will not be updated. Error: {ErrorMessage}",
                connection.PatreonConnectionId, identityResult.ErrorMessage);
        
        var updateTokensResult = await patreonService.UpdatePatreonConnectionTokens(
            connection.PatreonConnectionId,
            tokenResponse.RefreshToken, 
            tokenResponse.AccessToken, 
            tokenResponse.TokenType,
            DateTime.Now.AddSeconds(tokenResponse.ExpiresIn), 
            tokenResponse.Scope);
        if (!updateTokensResult.IsSuccessful)
        {
            logger.LogError("RefreshTask {PatreonConnectionId}: Failed to update Patreon tokens in database. Unlinking this connection to prevent future errors. Error: {ErrorMessage}", connection.PatreonConnectionId, updateTokensResult.ErrorMessage);
            _ = patreonService.DeletePatreonConnection(connection.PatreonConnectionId);
            return false;
        }
        
        if (identityResult.IsSuccessful && (latestFullName != connection.FullName || latestPledge != connection.Pledge))
        {
            var updateProfileResult = await patreonService.UpdatePatreonConnectionProfile(connection.PatreonConnectionId, latestFullName, latestPledge);
            if (!updateProfileResult.IsSuccessful)
                logger.LogWarning("RefreshTask {PatreonConnectionId}: Failed to update Patreon profile. Error: {ErrorMessage}", connection.PatreonConnectionId, updateProfileResult.ErrorMessage);
        }
        
        logger.LogInformation("RefreshTask {PatreonConnectionId}: Successfully refreshed Patreon token.", connection.PatreonConnectionId);
        return true;
    }
    
}