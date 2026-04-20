using GreenfieldCoreServices.Models.Redblocks;

namespace GreenfieldCoreServices.Services.Caching;

public class RedblockProjectCache : BaseCacheService<string, RedblockProject>;

public class RedblockCache : BaseCacheService<string, Redblock>;