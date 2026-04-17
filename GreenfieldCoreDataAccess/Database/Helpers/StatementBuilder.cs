using Dapper;

namespace GreenfieldCoreDataAccess.Database.Helpers;

public class StatementBuilder : BaseStatementPartBuilder<StatementBuilder, (string query,  DynamicParameters parameters)>
{
    private readonly List<StatementPart> _parts = new();
    private string _from = string.Empty;
    private string _action = string.Empty;

    private StatementBuilder() { }

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
    
    public StatementBuilder WithPart(Func<IStatementPartBuilder<StatementPartBuilder, StatementPart>, IStatementPartBuilder<StatementPartBuilder, StatementPart>> partFunc)
    {
        var part = partFunc(new StatementPartBuilder());
        _parts.Add(part.Build());
        return this;
    }

    public override (string query, DynamicParameters parameters) Build()
    {
        var allColumnParts = _part.ColumnPart;
        var allJoinParts = _part.JoinPart;
        var allWhereParts = _part.WherePart;
        var allHavingParts = _part.HavingPart;
        var allOrderParts = _part.OrderPart;
        var parameters = new DynamicParameters(_part.Parameters);
        
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
                        $"{(string.IsNullOrEmpty(allOrderParts) ? string.Empty : "ORDER BY " + allOrderParts)}";
        return (statement, parameters);
    }
}

public class StatementPartBuilder : BaseStatementPartBuilder<StatementPartBuilder, StatementPart>
{
    public override StatementPart Build()
    {
        return _part;
    }
}