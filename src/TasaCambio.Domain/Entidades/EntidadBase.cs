namespace TasaCambio.Domain.Entidades;

public abstract class EntidadBase
{
    public long Id { get; protected set; }
    public string UsuarioReg { get; set; } = string.Empty;
    public DateTime FechaReg { get; set; } = DateTime.UtcNow;
    public string? UsuarioAct { get; set; }
    public DateTime? FechaAct { get; set; }
}
