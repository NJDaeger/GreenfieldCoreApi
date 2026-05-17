using System.Net;
using System.Text.Json;
using GreenfieldCoreDataAccess.Database.UnitOfWork;
using GreenfieldCoreServices.Services.External.Interfaces;
using Microsoft.Extensions.Logging;

namespace GreenfieldCoreServices.Services.External;

public class MojangApi(ILogger<IMojangApi> logger, HttpClient client) : IMojangApi
{
    private sealed record MojangProfileResponse(string id, string name);

    public async Task<Result<string>> GetCurrentUsername(Guid minecraftUuid)
    {
        var compactUuid = minecraftUuid.ToString("N");
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri($"/session/minecraft/profile/{compactUuid}", UriKind.Relative));
        var response = await client.SendAsync(request);

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Result<string>.Failure($"Minecraft profile was not found for UUID '{minecraftUuid}'.", HttpStatusCode.NotFound);

        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to retrieve Mojang profile. StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            return Result<string>.Failure($"Failed to retrieve Mojang profile. {response.ReasonPhrase}", response.StatusCode);
        }

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            var model = JsonSerializer.Deserialize<MojangProfileResponse>(content);
            if (model is null || string.IsNullOrWhiteSpace(model.name))
                return Result<string>.Failure("Failed to deserialize Mojang profile response.", HttpStatusCode.BadGateway);

            return Result<string>.Success(model.name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception occurred while deserializing Mojang profile response.");
            return Result<string>.Failure($"Exception occurred while deserializing Mojang profile response: {ex.Message}", HttpStatusCode.BadGateway);
        }
    }
}

