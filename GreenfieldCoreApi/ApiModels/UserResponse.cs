namespace GreenfieldCoreModels.ApiModels.User;

public class UserResponse
{
    
    /// <summary>
    /// The users minecraft uuid
    /// </summary>
    public required Guid MinecraftUuid { get; set; }
    
    /// <summary>
    /// The users minecraft username
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// The users display name
    /// </summary>
    public string? DisplayName { get; set; }
    
}