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
    long? DeletedBy,
    DateTime? DeletedOn,
    long? LatestStatusId,
    string? LatestStatus,
    long? LatestStatusCreatedBy,
    DateTime? LatestStatusCreatedOn);

public record RedblockStatusEntity(
    long StatusId,
    long ProjectId,
    long KeyNumber,
    long RedblockId,
    string Status,
    long CreatedBy,
    DateTime CreatedOn);

public record RedblockUserAssignmentEntity(
    long UserAssignmentId,
    long ProjectId,
    long KeyNumber,
    long RedblockId,
    long AssignedTo,
    long CreatedBy,
    DateTime CreatedOn);

public record RedblockRoleAssignmentEntity(
    long RoleAssignmentId,
    long ProjectId,
    long KeyNumber,
    long RedblockId,
    string RoleName,
    long CreatedBy,
    DateTime CreatedOn);

