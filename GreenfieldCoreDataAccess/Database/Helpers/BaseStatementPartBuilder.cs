using Dapper;

namespace GreenfieldCoreDataAccess.Database.Helpers;

public abstract class BaseStatementPartBuilder<TBuilder, TBuilt> : IStatementPartBuilder<TBuilder, TBuilt> where TBuilder : BaseStatementPartBuilder<TBuilder, TBuilt>
{
    protected StatementPart Part = new();
    
    /// <summary>
    /// Sets the WHERE clause for the statement part.
    /// </summary>
    /// <param name="whereClause"></param>
    /// <returns></returns>
    public TBuilder Where(string? whereClause)
    {
        Part.WherePart = whereClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the JOIN clause for the statement part.
    /// </summary>
    /// <param name="joinClause"></param>
    /// <returns></returns>
    public TBuilder Join(string? joinClause)
    {
        Part.JoinPart = joinClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the ORDER BY clause for the statement part.
    /// </summary>
    /// <param name="orderClause"></param>
    /// <returns></returns>
    public TBuilder OrderBy(string? orderClause)
    {
        Part.OrderPart = orderClause;
        return (TBuilder)this; }
    
    /// <summary>
    /// Sets the column clause for the statement part, typically used in SELECT statements to specify which columns to retrieve.
    /// </summary>
    /// <param name="columnClause"></param>
    /// <returns></returns>
    public TBuilder Columns(string? columnClause)
    { 
        Part.ColumnPart = columnClause;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the HAVING clause for the statement part
    /// </summary>
    /// <param name="havingClause"></param>
    /// <returns></returns>
    public TBuilder Having(string? havingClause)
    {
        Part.HavingPart = havingClause;
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
        Part.Parameters[name] = value;
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
        Part.Parameters[name + Part.Parameters.Count] = value;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Builds the statement part
    /// </summary>
    /// <returns></returns>
    public abstract TBuilt Build();
}