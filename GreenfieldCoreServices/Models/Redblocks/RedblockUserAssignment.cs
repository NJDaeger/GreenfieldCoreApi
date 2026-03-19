using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public record RedblockUserAssignment(
    long UserAssignmentId,
    long ProjectId,
    long KeyNumber,
    long RedblockId,
    long AssignedTo,
    long CreatedBy,
    DateTime CreatedOn) : IModelConvertable<RedblockUserAssignmentEntity, RedblockUserAssignment>
{
    public static RedblockUserAssignment FromModel(RedblockUserAssignmentEntity from)
    {
        return new RedblockUserAssignment(
            from.UserAssignmentId,
            from.ProjectId,
            from.KeyNumber,
            from.RedblockId,
            from.AssignedTo,
            from.CreatedBy,
            from.CreatedOn);
    }
}
