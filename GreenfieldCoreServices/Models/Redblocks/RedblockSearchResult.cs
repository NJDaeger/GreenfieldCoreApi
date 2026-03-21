namespace GreenfieldCoreServices.Models.Redblocks;

public class RedblockSearchResult
{
    public required List<Redblock> FoundRedblocks = [];
    public required List<FailedRedblockLookup> FailedRedblockLookups = [];
}

public class FailedRedblockLookup
{
    public required Redblock Redblock;
    public required string FailureReason;
}