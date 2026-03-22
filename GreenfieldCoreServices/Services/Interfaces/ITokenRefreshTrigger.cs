namespace GreenfieldCoreServices.Services.Interfaces;

public interface ITokenRefreshTrigger
{
    Task TriggerAsync(CancellationToken cancellationToken);
}

