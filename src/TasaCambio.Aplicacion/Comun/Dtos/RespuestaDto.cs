namespace TasaCambio.Aplicacion.Comun.Dtos;

public sealed record RespuestaDto<T>
{
    public bool Exitoso { get; init; }
    public T? Datos { get; init; }
    public string? Mensaje { get; init; }
    public IReadOnlyList<string> Errores { get; init; } = [];

    public static RespuestaDto<T> Ok(T datos, string? mensaje = null) =>
        new() { Exitoso = true, Datos = datos, Mensaje = mensaje };

    public static RespuestaDto<T> Fallo(string error) =>
        new() { Exitoso = false, Errores = [error] };

    public static RespuestaDto<T> Fallo(IReadOnlyList<string> errores) =>
        new() { Exitoso = false, Errores = errores };
}
