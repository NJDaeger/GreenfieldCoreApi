namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockSearchResult
{
    /// <summary>
    /// List of redblock identifiers matching the search criteria.
    /// Each identifier can be used to fetch full details via the detail endpoint.
    /// </summary>
    public required List<RedblockSearchIdentifier> Results { get; set; } = [];
    
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

    /// <summary>
    /// List of redblocks that failed to process during search, with failure reasons.
    /// </summary>
    public required List<FailedRedblockLookup> FailedRedblockLookups { get; set; } = [];
}

public class FailedRedblockLookup
{
    public required Redblock Redblock;
    public required string FailureReason;
}