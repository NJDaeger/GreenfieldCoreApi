using Dapper;

namespace GreenfieldCoreDataAccess.Database.Helpers;

public class StatementBuilder : BaseStatementPartBuilder<StatementBuilder, (string query,  DynamicParameters parameters)>
{
    private readonly List<StatementPart> _parts = new();
    private string _from = string.Empty;
    private string _action = string.Empty;
    private long? _limit;
    private long? _offset;

    private StatementBuilder() { }

    private StatementBuilder(StatementBuilder copy)
    {        
        _parts = new List<StatementPart>(copy._parts);
        _from = copy._from;
        _action = copy._action;
        _limit = copy._limit;
        _offset = copy._offset;
        Part = copy.Part;
    }
    
    public static StatementBuilder SelectFrom(string from)
    {
        var builder = new StatementBuilder
        {
            _action = "SELECT",
            _from = from
        };
        return builder;
    }
    
    public StatementBuilder WithPart(StatementPart part)
    {
        _parts.Add(part);
        return this;
    }

    public StatementBuilder WithLimit(long limit)
    {
        _limit = limit;
        return this;
    }

    public StatementBuilder WithOffset(long offset)
    {
        _offset = offset;
        return this;
    }
    
    public StatementBuilder WithPart(Func<IStatementPartBuilder<StatementPartBuilder, StatementPart>, IStatementPartBuilder<StatementPartBuilder, StatementPart>> partFunc)
    {
        var part = partFunc(new StatementPartBuilder());
        _parts.Add(part.Build());
        return this;
    }

    public (string query, DynamicParameters parameters) BuildCount()
    {
        var copy = new StatementBuilder(this);
        copy._limit = null;
        copy._offset = null;
        var (innerQuery, parameters) = copy.Build();
        var statement = $"SELECT COUNT(*) FROM ({innerQuery}) AS CountSubquery";
        return (statement, parameters);
    }

    public override (string query, DynamicParameters parameters) Build()
    {
        var allColumnParts = Part.ColumnPart;
        var allJoinParts = Part.JoinPart;
        var allWhereParts = Part.WherePart;
        var allHavingParts = Part.HavingPart;
        var allOrderParts = Part.OrderPart;
        var parameters = new DynamicParameters(Part.Parameters);
        
        foreach (var part in _parts)
        {
            if (!string.IsNullOrEmpty(part.ColumnPart))
                allColumnParts = string.IsNullOrEmpty(allColumnParts) ? part.ColumnPart : $"{allColumnParts}, {part.ColumnPart}";
            
            if (!string.IsNullOrEmpty(part.JoinPart))
                allJoinParts = string.IsNullOrEmpty(allJoinParts) ? part.JoinPart : $"{allJoinParts} {part.JoinPart}";
            
            if (!string.IsNullOrEmpty(part.WherePart))
                allWhereParts = string.IsNullOrEmpty(allWhereParts) ? part.WherePart : $"{allWhereParts} {part.WherePart}";
            
            if (!string.IsNullOrEmpty(part.HavingPart))
                allHavingParts = string.IsNullOrEmpty(allHavingParts) ? part.HavingPart : $"{allHavingParts} {part.HavingPart}";
            
            if (!string.IsNullOrEmpty(part.OrderPart))
                allOrderParts = string.IsNullOrEmpty(allOrderParts) ? part.OrderPart : $"{allOrderParts}, {part.OrderPart}";
            
            foreach (var param in part.Parameters)
                parameters.Add(param.Key, param.Value);
        }
        
        var statement = $"{_action} {(string.IsNullOrEmpty(allColumnParts) ? "*" : allColumnParts)} FROM {_from} " +
                        $"{(string.IsNullOrEmpty(allJoinParts) ? string.Empty : allJoinParts)} " +
                        $"{(string.IsNullOrEmpty(allWhereParts) ? string.Empty : "WHERE " + allWhereParts)} " +
                        $"{(string.IsNullOrEmpty(allHavingParts) ? string.Empty : "HAVING " + allHavingParts)} " +
                        $"{(string.IsNullOrEmpty(allOrderParts) ? string.Empty : "ORDER BY " + allOrderParts)} " +
                        $"{(_limit.HasValue ? "LIMIT " + _limit.Value : string.Empty)} " +
                        $"{(_offset.HasValue ? "OFFSET " + _offset.Value : string.Empty)}";
        return (statement, parameters);
    }
}

public class StatementPartBuilder : BaseStatementPartBuilder<StatementPartBuilder, StatementPart>
{
    public override StatementPart Build()
    {
        return Part;
    }
}