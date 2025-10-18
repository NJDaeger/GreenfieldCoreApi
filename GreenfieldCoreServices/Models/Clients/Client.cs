namespace GreenfieldCoreServices.Models.Clients;

public class Client
{
    public required Guid ClientId { get; set; }
    public required string ClientName { get; set; }
    public required DateTime CreatedOn { get; set; }
    
    public required List<string> Roles { get; set; }

}