using GreenfieldCoreServices.Models.Connections.Discord;
using GreenfieldCoreServices.Services.External.Interfaces;
using GreenfieldCoreServices.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.Tasks;

public class DiscordTokenRefreshTask(TaskStartSignalService startSignal, IServiceScopeFactory scopeFactory, ILogger<DiscordTokenRefreshTask> logger) : BackgroundService, ITokenRefreshTrigger
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await startSignal.WaitForStartAsync(stoppingToken);

        await RefreshDiscordTokensAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(12), stoppingToken);
                await RefreshDiscordTokensAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during the Discord token refresh task.");
            }
        }
    }

    public Task TriggerAsync(CancellationToken cancellationToken) => RefreshDiscordTokensAsync(cancellationToken);

    private async Task RefreshDiscordTokensAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var discordService = scope.ServiceProvider.GetRequiredService<IDiscordService>();

        logger.LogInformation("Starting Discord token refresh task at {Time}", DateTimeOffset.Now);
        var connectionsResult = await discordService.GetAllDiscordConnections();
        if (!connectionsResult.TryGetDataNonNull(out var connections))
        {
            logger.LogWarning("Failed to retrieve Discord accounts for token refresh. Error: {ErrorMessage}", connectionsResult.ErrorMessage);
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
                var semDiscordService = semScope.ServiceProvider.GetRequiredService<IDiscordService>();
                var semDiscordApi = semScope.ServiceProvider.GetRequiredService<IDiscordApi>();
                try
                {
                    if (await RefreshConnection(connection, semDiscordApi, semDiscordService)) 
                        Interlocked.Increment(ref totalRefreshed);
                }
                finally
                {
                    semaphore.Release();
                }
            }, cancellationToken));
        }

        await Task.WhenAll(refreshTasks);
        logger.LogInformation("Completed Discord token refresh task at {Time}. Refreshed {Count} tokens.", DateTimeOffset.Now, totalRefreshed);
    }

    private async Task<bool> RefreshConnection(DiscordConnection connection, IDiscordApi discordApi, IDiscordService discordService)
    {
        var expiresIn = connection.RefreshBy - DateTime.Now;
        if (connection.RefreshBy <= DateTime.Now)
        {
            logger.LogWarning("RefreshTask {DiscordConnectionId}: Discord token expired. All linked users will be unlinked.", connection.DiscordConnectionId);
            await discordService.DeleteDiscordConnection(connection.DiscordConnectionId);
            return false;
        }

        if (expiresIn > TimeSpan.FromDays(2))
        {
            logger.LogDebug("RefreshTask {DiscordConnectionId}: Discord token not close to expiry (expires in {ExpiresIn}). Skipping refresh.", connection.DiscordConnectionId, expiresIn);
            return false;   
        }

        var refreshResult = await discordApi.RefreshDiscordAccessTokenAsync(connection.RefreshToken);
        if (!refreshResult.TryGetDataNonNull(out var tokenResponse))
        {
            logger.LogError("RefreshTask {DiscordConnectionId}: Failed to refresh Discord token. Error: {ErrorMessage}", connection.DiscordConnectionId, refreshResult.ErrorMessage);
            return false;
        }

        var latestUsername = connection.DiscordUsername;
        var identityResult = await discordApi.GetDiscordIdentity(tokenResponse.AccessToken);
        if (identityResult.TryGetDataNonNull(out var identity))
            latestUsername = identity.GlobalName ?? identity.Username;
        else
            logger.LogWarning("RefreshTask {DiscordConnectionId}: Failed to fetch Discord identity. Their non-token information will not be updated. Error: {ErrorMessage}", connection.DiscordConnectionId, identityResult.ErrorMessage);

        var updateTokensResult = await discordService.UpdateDiscordConnectionTokens(
            connection.DiscordConnectionId,
            tokenResponse.RefreshToken,
            tokenResponse.AccessToken,
            tokenResponse.TokenType,
            DateTime.Now.AddSeconds(tokenResponse.ExpiresIn),
            tokenResponse.Scope);
        if (!updateTokensResult.IsSuccessful)
        {
            logger.LogError("RefreshTask {DiscordConnectionId}: Failed to update Discord tokens in database. Unlinking this connection to prevent future errors. Error: {ErrorMessage}", connection.DiscordConnectionId, updateTokensResult.ErrorMessage);
            await discordService.DeleteDiscordConnection(connection.DiscordConnectionId);
            return false;
        }

        if (identityResult.IsSuccessful && latestUsername != connection.DiscordUsername)
        {
            var updateProfileResult = await discordService.UpdateDiscordConnectionProfile(connection.DiscordConnectionId, latestUsername);
            if (!updateProfileResult.IsSuccessful)
                logger.LogWarning("RefreshTask {DiscordConnectionId}: Failed to update Discord profile. Error: {ErrorMessage}", connection.DiscordConnectionId, updateProfileResult.ErrorMessage);
        }

        logger.LogInformation("RefreshTask {DiscordConnectionId}: Successfully refreshed Discord token.", connection.DiscordConnectionId);
        return true;
    }
    
}
