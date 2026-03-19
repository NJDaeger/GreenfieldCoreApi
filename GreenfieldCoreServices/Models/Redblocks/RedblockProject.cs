using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockProject : IModelConvertable<RedblockProjectEntity, RedblockProject>
{
    public required long ProjectId { get; set; }
    public required string ProjectName { get; set; }
    public required string ProjectKey { get; set; }
    public required long LastUsedRedblockKeyNumber { get; set; }

    public static RedblockProject FromModel(RedblockProjectEntity from)
    {
        return new RedblockProject
        {
            ProjectId = from.ProjectId,
            ProjectName = from.ProjectName,
            ProjectKey = from.ProjectKey,
            LastUsedRedblockKeyNumber = from.LastUsedRedblockKeyNumber
        };
    }
}
