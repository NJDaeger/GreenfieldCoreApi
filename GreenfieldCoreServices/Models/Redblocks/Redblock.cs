using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public class Redblock : IModelConvertable<(RedblockEntity redblockEntity, RedblockProject project, List<RedblockStatus> statuses, List<RedblockUserAssignment> userAssignments, List<RedblockRoleAssignment> roleAssignments, List<Guid> entities), Redblock>
{
    public required string Key { get; set; }
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
    public List<Guid> Entities { get; set; } = [];

    public static Redblock FromModel((RedblockEntity redblockEntity, RedblockProject project, List<RedblockStatus> statuses, List<RedblockUserAssignment> userAssignments, List<RedblockRoleAssignment> roleAssignments, List<Guid> entities) from)
    {
        return new Redblock
        {
            Key = from.project.ProjectKey + "-" + from.redblockEntity.KeyNumber,
            Message = from.redblockEntity.Message,
            X = from.redblockEntity.X,
            Y = from.redblockEntity.Y,
            Z = from.redblockEntity.Z,
            Project = from.project,
            Statuses = from.statuses,
            UserAssignments = from.userAssignments,
            RoleAssignments = from.roleAssignments,
            Entities = from.entities,
            UpdatedBy = from.redblockEntity.UpdatedBy,
            UpdatedOn = from.redblockEntity.UpdatedOn,
            CreatedBy = from.redblockEntity.CreatedBy,
            CreatedOn = from.redblockEntity.CreatedOn,
            DeletedBy = from.redblockEntity.DeletedBy,
            DeletedOn = from.redblockEntity.DeletedOn
        };
    }
}
