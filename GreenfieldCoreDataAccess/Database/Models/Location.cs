namespace GreenfieldCoreDataAccess.Database.Models;

public struct Location(int x, int y, int z)
{
    public long X { get; set; } = x;
    public long Y { get; set; } = y;
    public long Z { get; set; } = z;
}