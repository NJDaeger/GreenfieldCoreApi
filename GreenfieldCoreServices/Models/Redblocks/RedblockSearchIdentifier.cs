namespace GreenfieldCoreServices.Models.Redblocks;

/// <summary>
/// A lightweight identifier for a redblock used in search results.
/// Contains only the information needed to fetch full details via the detail endpoint.
/// </summary>
public class RedblockSearchIdentifier
{
    /// <summary>
    /// The internal redblock ID (used for cursor-based pagination).
    /// </summary>
    public required long RedblockId { get; set; }

    /// <summary>
    /// The project key (short identifier).
    /// </summary>
    public required string ProjectKey { get; set; }

    /// <summary>
    /// The redblock's key number within the project.
    /// </summary>
    public required long KeyNumber { get; set; }
}

