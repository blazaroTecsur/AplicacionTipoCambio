using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TasaCambio.Domain.Entidades;

namespace TasaCambio.Infrastructure.Persistencia.Configuraciones;

internal sealed class MonedaConfiguracion : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("ttc_moneda");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("ttc_id").ValueGeneratedOnAdd();

        builder.Property(x => x.Codigo).HasColumnName("ttc_codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("ttc_descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Simbolo).HasColumnName("ttc_simbolo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.CodigoSunat).HasColumnName("ttc_codigo_sunat").HasMaxLength(10);
        builder.Property(x => x.DescripcionIso4217).HasColumnName("ttc_descripcion_iso4217").HasMaxLength(50);

        builder.Property(x => x.UsuarioReg).HasColumnName("ttc_usuario_reg").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaReg).HasColumnName("ttc_fecha_reg").IsRequired();
        builder.Property(x => x.UsuarioAct).HasColumnName("ttc_usuario_act").HasMaxLength(50);
        builder.Property(x => x.FechaAct).HasColumnName("ttc_fecha_act");

        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("idx_ttc_moneda_codigo");
    }
}
