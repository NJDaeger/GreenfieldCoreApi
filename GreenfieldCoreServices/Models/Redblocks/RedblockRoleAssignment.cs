using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public record RedblockRoleAssignment(
    long RoleAssignmentId,
    long ProjectId,
    long KeyNumber,
    long RedblockId,
    string RoleName,
    long CreatedBy,
    DateTime CreatedOn) : IModelConvertable<RedblockRoleAssignmentEntity, RedblockRoleAssignment>
{
    public static RedblockRoleAssignment FromModel(RedblockRoleAssignmentEntity from)
    {
        return new RedblockRoleAssignment(
            from.RoleAssignmentId,
            from.ProjectId,
            from.KeyNumber,
            from.RedblockId,
            from.RoleName,
            from.CreatedBy,
            from.CreatedOn);
    }
}
