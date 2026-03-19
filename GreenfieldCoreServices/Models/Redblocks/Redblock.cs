using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public class Redblock : IModelConvertable<RedblockEntity, Redblock>
{
    public required long RedblockId { get; set; }
    public required long ProjectId { get; set; }
    public required long KeyNumber { get; set; }
    public required string Message { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Z { get; set; }
    public required long CreatedBy { get; set; }
    public required DateTime CreatedOn { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public long? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }
    
    public RedblockProject Project { get; set; } = null!;
    public List<RedblockStatus> Statuses { get; set; } = [];
    public List<RedblockUserAssignment> UserAssignments { get; set; } = [];
    public List<RedblockRoleAssignment> RoleAssignments { get; set; } = [];

    public static Redblock FromModel(RedblockEntity from)
    {
        return new Redblock
        {
            RedblockId = from.RedblockId,
            ProjectId = from.ProjectId,
            KeyNumber = from.KeyNumber,
            Message = from.Message,
            X = from.X,
            Y = from.Y,
            Z = from.Z,
            CreatedBy = from.CreatedBy,
            CreatedOn = from.CreatedOn,
            DeletedBy = from.DeletedBy,
            DeletedOn = from.DeletedOn
        };
    }
}
