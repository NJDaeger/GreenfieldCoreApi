namespace GreenfieldCoreDataAccess.Database.Models;

public record RedblockProjectEntity(
    long ProjectId,
    string ProjectName,
    string ProjectKey,
    long LastUsedRedblockKeyNumber);

public record RedblockEntity(
    long RedblockId,
    long ProjectId,
    long KeyNumber,
    string Message,
    int X,
    int Y,
    int Z,
    long CreatedBy,
    DateTime CreatedOn,
    long? UpdatedBy,
    DateTime? UpdatedOn,
    long? DeletedBy,
    DateTime? DeletedOn);

public record RedblockWithLatestStatusEntity : RedblockEntity
{
    
    public string Status { get; set; }
    public double? DistanceSquared { get; set; }

    public RedblockWithLatestStatusEntity(long redblockId, long projectId, long keyNumber, string message,
        string status, int x, int y, int z, long createdBy, DateTime createdOn, long? updatedBy, DateTime? updatedOn,
        long? deletedBy, DateTime? deletedOn) : base(redblockId, projectId, keyNumber, message, x, y, z, createdBy,
        createdOn, updatedBy, updatedOn, deletedBy, deletedOn)
    {
        Status = status;
    }
    
    public RedblockWithLatestStatusEntity(long redblockId, long projectId, long keyNumber, string message,
        string status, int x, int y, int z, long createdBy, DateTime createdOn, long? updatedBy, DateTime? updatedOn,
        long? deletedBy, DateTime? deletedOn, double? distanceSquared) : base(redblockId, projectId, keyNumber, message, x, y, z, createdBy,
        createdOn, updatedBy, updatedOn, deletedBy, deletedOn)
    {
        Status = status;
        DistanceSquared = distanceSquared;
    }
}

public record RedblockStatusEntity(
    long StatusId,
    long RedblockId,
    string Status,
    long CreatedBy,
    DateTime CreatedOn);

public record RedblockUserAssignmentEntity(
    long UserAssignmentId,
    long RedblockId,
    long AssignedTo,
    long CreatedBy,
    DateTime CreatedOn);

public record RedblockRoleAssignmentEntity(
    long RoleAssignmentId,
    long RedblockId,
    string RoleName,
    long CreatedBy,
    DateTime CreatedOn);

