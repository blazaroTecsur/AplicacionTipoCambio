using Microsoft.EntityFrameworkCore.Storage;
using TasaCambio.Domain.Interfaces;
using TasaCambio.Infrastructure.Persistencia.Repositorios;

namespace TasaCambio.Infrastructure.Persistencia;

internal sealed class UnidadDeTrabajo : IUnidadDeTrabajo
{
    private readonly TasaCambioDbContext _contexto;
    private IDbContextTransaction? _transaccion;

    public ITasaCambioRepositorio TasaCambios { get; }
    public IMonedaRepositorio Monedas { get; }

    public UnidadDeTrabajo(TasaCambioDbContext contexto)
    {
        _contexto = contexto;
        TasaCambios = new TasaCambioRepositorio(contexto);
        Monedas = new MonedaRepositorio(contexto);
    }

    public Task<int> GuardarCambiosAsync(CancellationToken ct = default)
        => _contexto.SaveChangesAsync(ct);

    public async Task IniciarTransaccionAsync(CancellationToken ct = default)
        => _transaccion = await _contexto.Database.BeginTransactionAsync(ct);

    public async Task ConfirmarTransaccionAsync(CancellationToken ct = default)
    {
        if (_transaccion is null) return;
        await _transaccion.CommitAsync(ct);
        await _transaccion.DisposeAsync();
        _transaccion = null;
    }

    public async Task RevertirTransaccionAsync(CancellationToken ct = default)
    {
        if (_transaccion is null) return;
        await _transaccion.RollbackAsync(ct);
        await _transaccion.DisposeAsync();
        _transaccion = null;
    }

    public void Dispose()
    {
        _transaccion?.Dispose();
        _contexto.Dispose();
    }
}
