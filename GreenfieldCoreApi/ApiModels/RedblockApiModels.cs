namespace GreenfieldCoreApi.ApiModels;

public class CreateProjectRequest
{
    public required string ProjectName { get; set; }
}

public class UpdateProjectRequest
{
    public required string ProjectName { get; set; }
}

public class CreateRedblockRequest
{
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Z { get; set; }
    public required string Message { get; set; }
    public required long CreatedBy { get; set; }
    public required string InitialStatus { get; set; }
    public List<long> AssignedUsers { get; set; } = [];
    public List<string> AssignedRoles { get; set; } = [];
}

public class UpdateRedblockRequest
{
    public required string Message { get; set; }
    public required long UpdatedBy { get; set; }
}

public class DeleteRedblockRequest
{
    public required long DeletedBy { get; set; }
}

public class AddStatusRequest
{
    public required string Status { get; set; }
    public required long CreatedBy { get; set; }
}

public class AssignUserRequest
{
    public required long UserId { get; set; }
    public required long CreatedBy { get; set; }
}

public class AssignRoleRequest
{
    public required string RoleName { get; set; }
    public required long CreatedBy { get; set; }
}

public class ReplaceRedblockEntitiesRequest
{
    public required List<Guid> Entities { get; init; } = [];
}

