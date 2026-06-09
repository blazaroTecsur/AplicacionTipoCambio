using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TasaCambio.Infrastructure.Persistencia.Configuraciones;

internal sealed class TasaCambioConfiguracion : IEntityTypeConfiguration<Domain.Entidades.TasaCambio>
{
    public void Configure(EntityTypeBuilder<Domain.Entidades.TasaCambio> builder)
    {
        builder.ToTable("ttc_tasa_cambio");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ttc_id").ValueGeneratedOnAdd();

        builder.Property(x => x.Empresa).HasColumnName("ttc_empresa").HasMaxLength(20).IsRequired();
        builder.Property(x => x.CodigoMoneda).HasColumnName("ttc_codigo_moneda").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Fecha).HasColumnName("ttc_fecha").IsRequired();
        builder.Property(x => x.ValorCompra).HasColumnName("ttc_valor_compra").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.ValorVenta).HasColumnName("ttc_valor_venta").HasPrecision(18, 6).IsRequired();
        builder.Property(x => x.FechaSbs).HasColumnName("ttc_fecha_sbs");
        builder.Property(x => x.FuenteOrigen).HasColumnName("ttc_fuente_origen").HasMaxLength(50);

        builder.Property(x => x.UsuarioReg).HasColumnName("ttc_usuario_reg").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaReg).HasColumnName("ttc_fecha_reg").IsRequired();
        builder.Property(x => x.UsuarioAct).HasColumnName("ttc_usuario_act").HasMaxLength(50);
        builder.Property(x => x.FechaAct).HasColumnName("ttc_fecha_act");

        builder.HasIndex(x => new { x.Empresa, x.CodigoMoneda, x.Fecha })
            .IsUnique()
            .HasDatabaseName("idx_ttc_empresa_moneda_fecha");

        builder.Ignore(x => x.TasaPromedio);
    }
}
