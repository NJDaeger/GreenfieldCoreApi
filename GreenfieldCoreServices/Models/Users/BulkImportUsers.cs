namespace GreenfieldCoreServices.Models.Users;

public class BulkImportUsersRequest
{
    public required IEnumerable<BulkImportUserEntry> Users { get; init; }
}

public class BulkImportUserEntry
{
    public required Guid Uuid { get; init; }
    public required string Username { get; init; }
}

public class BulkImportUserSkipped
{
    public required Guid Uuid { get; init; }
    public required string Reason { get; init; }
}

public class BulkImportUsersResult
{
    public required IEnumerable<Guid> Created { get; init; }
    public required IEnumerable<BulkImportUserSkipped> Skipped { get; init; }
}

