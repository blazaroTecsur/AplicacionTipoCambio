namespace TasaCambio.Aplicacion.Comun.Dtos;

public sealed record PaginadoDto<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalRegistros { get; init; }
    public int Pagina { get; init; }
    public int TamanoPagina { get; init; }
    public int TotalPaginas => (int)Math.Ceiling(TotalRegistros / (double)TamanoPagina);
}
