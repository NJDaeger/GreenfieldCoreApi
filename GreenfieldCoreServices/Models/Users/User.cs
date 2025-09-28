namespace GreenfieldCoreServices.Models.Users;

public class User
{
    
    /// <summary>
    /// The id used in the database
    /// </summary>
    public required long UserId { get; set; }
   
    /// <summary>
    /// This user's minecraft account uuid
    /// </summary>
    public required Guid MinecraftUuid { get; set; }
    
    /// <summary>
    /// This user's last known minecraft account username
    /// </summary>
    public required string Username { get; set; }
    
    /// <summary>
    /// This user's last known username
    /// </summary>
    public string? DisplayName { get; set; }
    
}