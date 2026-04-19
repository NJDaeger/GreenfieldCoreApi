using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public record RedblockStatus(
    long StatusId,
    long RedblockId,
    string Status,
    long CreatedBy,
    DateTime CreatedOn) : IModelConvertable<RedblockStatusEntity, RedblockStatus>
{
    public static RedblockStatus FromModel(RedblockStatusEntity from)
    {
        return new RedblockStatus(
            from.StatusId,
            from.RedblockId,
            from.Status,
            from.CreatedBy,
            from.CreatedOn);
    }
}
