namespace GreenfieldCoreDataAccess.Database.Helpers;

public class StatementPart
{
    public string? JoinPart { get; set; }
    
    public string? WherePart { get; set; }
    
    public string? OrderPart { get; set; }
    
    public string? ColumnPart { get; set; }
    
    public string? HavingPart { get; set; }
    
    public Dictionary<string, object> Parameters { get; set; } = new();

    /// <summary>
    /// Returns a comma-separated string of the parameter keys in this statement part.
    /// </summary>
    /// <returns></returns>
    public string JoinParameterKeys()
    {
        return string.Join(", ", Parameters.Keys);
    }
}