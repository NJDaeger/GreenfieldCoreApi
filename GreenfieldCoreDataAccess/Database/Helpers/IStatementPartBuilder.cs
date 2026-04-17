namespace GreenfieldCoreDataAccess.Database.Helpers;

public interface IStatementPartBuilder<out TBuilder, out TBuilt> where TBuilder : IStatementPartBuilder<TBuilder, TBuilt>
{
    TBuilder Where(string? whereClause);
    TBuilder Join(string? joinClause);
    TBuilder OrderBy(string? orderByClause);
    TBuilder Having(string? havingClause);
    TBuilder Columns(string? columnClause);
    TBuilder WithParameter(string name, object value);
    TBuilt Build();
    
}
