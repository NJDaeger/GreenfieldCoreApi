using GreenfieldCoreDataAccess.Database.Models;

namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockSearchResult
{
    /// <summary>
    /// List of redblock identifiers matching the search criteria.
    /// Each identifier can be used to fetch full details via the detail endpoint.
    /// </summary>
    public required List<SearchedRedblockResult> Results { get; set; } = [];
    
    /// <summary>
    /// Whether there are more results available after this page.
    /// Use NextCursorRedblockId in a new search request to fetch the next page.
    /// </summary>
    public required bool HasMore { get; set; }
    
    /// <summary>
    /// The RedblockId to use as SearchAfterRedblockId to fetch the next page.
    /// Only set if HasMore is true.
    /// </summary>
    public long? NextCursorRedblockId { get; set; }
    
    /// <summary>
    /// Number of results returned in this page.
    /// </summary>
    public required int ReturnedCount { get; set; }
}

/// <summary>
/// A lightweight identifier for a redblock used in search results.
/// Contains only the information needed to fetch full details via the detail endpoint.
/// </summary>
public class SearchedRedblockResult : IModelConvertable<RedblockWithLatestStatusEntity, SearchedRedblockResult>
{
    public required long RedblockId { get; set; }
    public required long ProjectId { get; set; }
    public required long KeyNumber { get; set; }
    public required string Message { get; set; }
    public required string Status { get; set; }
    public required int X { get; set; }
    public required int Y { get; set; }
    public required int Z { get; set; }
    public required long CreatedBy { get; set; }
    public required DateTime CreatedOn { get; set; }
    public long? UpdatedBy { get; set; }
    public DateTime? UpdatedOn { get; set; }
    public long? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    public double? Distance
    {
        get => field is null ? null : Math.Sqrt(field.Value);
        set;
    }

    public static SearchedRedblockResult FromModel(RedblockWithLatestStatusEntity from)
    {
        return new SearchedRedblockResult
        {
            RedblockId = from.RedblockId,
            ProjectId = from.ProjectId,
            KeyNumber = from.KeyNumber,
            Message = from.Message,
            Status = from.Status,
            X = from.X,
            Y = from.Y,
            Z = from.Z,
            CreatedBy = from.CreatedBy,
            CreatedOn = from.CreatedOn,
            UpdatedBy = from.UpdatedBy,
            UpdatedOn = from.UpdatedOn,
            DeletedBy = from.DeletedBy,
            DeletedOn = from.DeletedOn,
            Distance = from.DistanceSquared
        };
    }
}
