using Dapper;

namespace GreenfieldCoreDataAccess.Database.Helpers;

public abstract class BaseStatementPartBuilder<TBuilder, TBuilt> : IStatementPartBuilder<TBuilder, TBuilt> where TBuilder : BaseStatementPartBuilder<TBuilder, TBuilt>
{
    protected readonly StatementPart _part = new();
    
    /// <summary>
    /// Sets the WHERE clause for the statement part.
    /// </summary>
    /// <param name="whereClause"></param>
    /// <returns></returns>
    public TBuilder Where(string? whereClause)
    {
        _part.WherePart = whereClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the JOIN clause for the statement part.
    /// </summary>
    /// <param name="joinClause"></param>
    /// <returns></returns>
    public TBuilder Join(string? joinClause)
    {
        _part.JoinPart = joinClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the ORDER BY clause for the statement part.
    /// </summary>
    /// <param name="orderClause"></param>
    /// <returns></returns>
    public TBuilder OrderBy(string? orderClause)
    {
        _part.OrderPart = orderClause;
        return (TBuilder)this; }
    
    /// <summary>
    /// Sets the column clause for the statement part, typically used in SELECT statements to specify which columns to retrieve.
    /// </summary>
    /// <param name="columnClause"></param>
    /// <returns></returns>
    public TBuilder Columns(string? columnClause)
    { 
        _part.ColumnPart = columnClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the HAVING clause for the statement part
    /// </summary>
    /// <param name="havingClause"></param>
    /// <returns></returns>
    public TBuilder Having(string? havingClause)
    {
        _part.HavingPart = havingClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds a parameter to the statement part's parameter collection.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public TBuilder WithParameter(string name, object value)
    {
        _part.Parameters[name] = value;
        return (TBuilder)this;
    }

    /// <summary>
    /// Adds a parameter to the statement part's parameter collection with an indexed name to avoid conflicts.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public TBuilder WithIndexedParameter(string name, object value)
    {
        _part.Parameters[name + _part.Parameters.Count] = value;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Builds the statement part
    /// </summary>
    /// <returns></returns>
    public abstract TBuilt Build();
}