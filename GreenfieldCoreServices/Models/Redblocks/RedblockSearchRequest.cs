namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockSearchRequest
{
    /// <summary>
    /// Only return redblocks that match the StatusFilter. If null, status is not used for filtering.
    /// </summary>
    public StatusFilter? StatusFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the DeletionFilter. If null, deletion status is not used for filtering.
    /// </summary>
    public DeletionFilter? DeletionFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the UserAssignmentFilter. If null, user assignments are not used for filtering.
    /// </summary>
    public UserAssignmentFilter? UserAssignmentFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the RoleAssignmentFilter. If null, role assignments are not used for filtering.
    /// </summary>
    public RoleAssignmentFilter? RoleAssignmentFilter { get; set; }
    
    /// <summary>
    /// Only return redblocks that match the MessageFilter. If null, message content is not used for filtering.
    /// </summary>
    public MessageFilter? MessageFilter { get; set; }

    /// <summary>
    /// Number of results to return per page. Defaults to 50. Maximum is 500.
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// The RedblockId to start after for cursor-based pagination. 
    /// If null, search starts from the beginning.
    /// Enables efficient pagination for large result sets.
    /// </summary>
    public long? SearchAfterRedblockId { get; set; }
}

/// <summary>
/// Filter a redblock by its status
/// </summary>
public class StatusFilter
{
    /// <summary>
    /// Statuses to filter by with this status filter.
    /// </summary>
    public required List<string> Statuses { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a status. Supported options are "or" or "not"
    /// </summary>
    public required string MatchType;
}

/// <summary>
/// Filter a redblock by its deletion status
/// </summary>
public class DeletionFilter
{
    /// <summary>
    /// Users to filter by with this deletion filter.
    /// </summary>
    public required List<long> Users { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a deletion user. Supported options are "or", "not", "and"
    /// </summary>
    public required string MatchType  { get; set; }
}

/// <summary>
/// Filter a redblock by its user assignments
/// </summary>
public class UserAssignmentFilter
{
    /// <summary>
    /// Users to filter by with this user assignment filter.
    /// </summary>
    public required List<long> Users { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a user assignment user. Supported options are "or", "not", "and"
    /// </summary>
    public required string MatchType  { get; set; }
}

/// <summary>
/// Filter a redblock by its role assignments
/// </summary>
public class RoleAssignmentFilter
{
    /// <summary>
    /// Roles to filter by with this role assignment filter.
    /// </summary>
    public required List<string> Roles { get; set; } = [];
    
    /// <summary>
    /// How to match a redblock with a role assignment role. Supported options are "or", "not", "and"
    /// </summary>
    public required string MatchType  { get; set; }
}

public class MessageFilter
{
    /// <summary>
    /// Message content to filter by with this message filter.
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// How to match a redblock with the message content. Supported options are:
    /// <br/>- "contains"
    /// <br/>- "exact"
    /// <br/>- "startsWith"
    /// <br/>- "endsWith"
    /// <br/>- "fuzzy"
    /// </summary>
    public required string MatchType { get; set; }
}