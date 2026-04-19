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
    /// Total number of results matching the search criteria across all pages.
    /// </summary>
    public required long TotalResults { get; set; }
}

/// <summary>
/// A lightweight identifier for a redblock used in search results.
/// Contains only the information needed to fetch full details via the detail endpoint.
/// </summary>
public class SearchedRedblockResult : IModelConvertable<(RedblockWithLatestStatusEntity entity, string projectKey), SearchedRedblockResult>
{
    public required string Key { get; set; }
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

    public static SearchedRedblockResult FromModel((RedblockWithLatestStatusEntity entity, string projectKey) from)
    {
        return new SearchedRedblockResult
        {
            Key = from.projectKey + "-" + from.entity.KeyNumber,
            Message = from.entity.Message,
            Status = from.entity.Status,
            X = from.entity.X,
            Y = from.entity.Y,
            Z = from.entity.Z,
            CreatedBy = from.entity.CreatedBy,
            CreatedOn = from.entity.CreatedOn,
            UpdatedBy = from.entity.UpdatedBy,
            UpdatedOn = from.entity.UpdatedOn,
            DeletedBy = from.entity.DeletedBy,
            DeletedOn = from.entity.DeletedOn,
            Distance = from.entity.DistanceSquared
        };
    }
}
