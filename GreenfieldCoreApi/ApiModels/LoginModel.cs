namespace GreenfieldCoreModels.ApiModels.User;

public class LoginModel
{
    
    public required Guid ClientId { get; set; }
    public required string ClientName { get; set; }
    public required string ClientSecret { get; set; }
    
}