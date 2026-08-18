namespace TasaCambio.Application.Comun.Dtos;

public sealed record PaginatedDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalRecords { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalRecords / (double)PageSize);
}
