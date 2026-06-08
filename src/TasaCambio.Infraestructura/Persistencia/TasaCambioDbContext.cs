using Microsoft.EntityFrameworkCore;
using TasaCambio.Dominio.Entidades;
using TasaCambio.Infraestructura.Persistencia.Configuraciones;

namespace TasaCambio.Infraestructura.Persistencia;

public sealed class TasaCambioDbContext : DbContext
{
    public TasaCambioDbContext(DbContextOptions<TasaCambioDbContext> options) : base(options) { }

    public DbSet<Dominio.Entidades.TasaCambio> TasaCambios => Set<Dominio.Entidades.TasaCambio>();
    public DbSet<Moneda> Monedas => Set<Moneda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TasaCambioDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
