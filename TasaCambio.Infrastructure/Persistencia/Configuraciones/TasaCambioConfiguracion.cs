using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TasaCambio.Infrastructure.Persistencia.Configuraciones;

internal sealed class TasaCambioConfiguracion : IEntityTypeConfiguration<Domain.Entidades.TasaCambio>
{
    public void Configure(EntityTypeBuilder<Domain.Entidades.TasaCambio> builder)
    {
        builder.ToTable("tttasacambio");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("IdTasaCambio").ValueGeneratedOnAdd();

        builder.Property(x => x.CodigoMoneda).HasColumnName("CodigoMoneda").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("Fecha").IsRequired();
        builder.Property(x => x.ValorCompra).HasColumnName("ValorCompra").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.ValorVenta).HasColumnName("ValorVenta").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.FechaSbs).HasColumnName("FechaSbs");
        builder.Property(x => x.FuenteOrigen).HasColumnName("FuenteOrigen").HasMaxLength(50);
        builder.Property(x => x.SincronizadoSyteline).HasColumnName("SincronizadoSyteline").HasDefaultValue(false).IsRequired();
        builder.Property(x => x.FechaUltSincSyteline).HasColumnName("FechaUltSincSyteline");

        builder.Property(x => x.UsuarioReg).HasColumnName("UsuarioReg").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaReg).HasColumnName("FechaReg").IsRequired();
        builder.Property(x => x.UsuarioAct).HasColumnName("UsuarioAct").HasMaxLength(50);
        builder.Property(x => x.FechaAct).HasColumnName("FechaAct");

        builder.HasIndex(x => new { x.CodigoMoneda, x.Fecha })
            .IsUnique()
            .HasDatabaseName("IdxTtTasaCambioMonedaFecha");

        builder.Ignore(x => x.TasaPromedio);
    }
}
