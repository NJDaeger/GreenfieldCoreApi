namespace GreenfieldCoreServices.Models.Redblocks;

public class BulkImportRedblocksRequest
{
    public required Dictionary<string, BulkImportRedblockEntry> Redblocks { get; init; }
    public required Dictionary<string, BulkImportProject> WorldProjects { get; init; }
}

public class BulkImportProject
{
    public required string ProjectName { get; init; }
    public required string ProjectKey { get; init; }
}

public class BulkImportRedblockEntry
{
    public required string Content { get; init; }
    public required string Status { get; init; }
    public required Guid CreatedBy { get; init; }
    public required long CreatedOn { get; init; }
    public Guid? CompletedBy { get; init; }
    public long? CompletedOn { get; init; }
    public Guid? ApprovedBy { get; init; }
    public long? ApprovedOn { get; init; }
    public Guid? AssignedTo { get; init; }
    public long? AssignedOn { get; init; }
    public required BulkImportRedblockLocation Location { get; init; }
    public string? MinRank { get; init; }
    public List<Guid>? Armorstands { get; init; }
}

public class BulkImportFollowupWorkItem
{
    public required string LegacyRedblockKey { get; init; }
    public required long ProjectId { get; init; }
    public required long RedblockId { get; init; }
    public required long RedblockKeyNumber { get; init; }
    public required long CreatedByUserId { get; init; }
    public long? AssignedUserId { get; init; }
    public string? AssignedUsername { get; init; }
    public string? Role { get; init; }
    public required List<Guid> Entities { get; init; }
    public required List<(string Status, long CreatedByUserId)> Statuses { get; init; }
    public required bool IsDeleted { get; init; }
    public required long SystemUserId { get; init; }
}

public class BulkImportRedblockLocation
{
    public required long X { get; init; }
    public required long Y { get; init; }
    public required long Z { get; init; }
    public required string World { get; init; }
}

public class BulkImportRedblocksResult
{
    public required IEnumerable<string> Errors { get; init; }
}

