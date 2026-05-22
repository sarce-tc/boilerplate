// Microservice.Application/Models/QueryParameters.cs
// Reemplaza Expression<Func<T, bool>> con algo que Dapper puede consumir

public sealed class QueryParameters<T>
{
    // Filtros tipados que Infrastructure traduce a WHERE clauses
    public Dictionary<string, object?> Filters { get; init; } = [];

    // Ordenamiento: nombre de columna + dirección
    public string? OrderByColumn { get; init; }
    public bool OrderDescending { get; init; }

    // Paginación
    public int? Page { get; init; }
    public int? PageSize { get; init; }

    // Proyección: qué columnas traer (evita SELECT *)
    public IReadOnlyList<string>? Columns { get; init; }

    // Factory methods legibles
    public static QueryParameters<T> Empty => new();

    public static QueryParameters<T> WithFilter(string column, object? value) =>
        new() { Filters = new() { [column] = value } };
}