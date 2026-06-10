using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TasaCambio.Domain.Entidades;

namespace TasaCambio.Infrastructure.Persistencia.Configuraciones;

internal sealed class MonedaConfiguracion : IEntityTypeConfiguration<Moneda>
{
    public void Configure(EntityTypeBuilder<Moneda> builder)
    {
        builder.ToTable("ttmoneda");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("IdMoneda").ValueGeneratedOnAdd();

        builder.Property(x => x.Codigo).HasColumnName("Codigo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.Descripcion).HasColumnName("Descripcion").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Simbolo).HasColumnName("Simbolo").HasMaxLength(10).IsRequired();
        builder.Property(x => x.CodigoSunat).HasColumnName("CodigoSunat").HasMaxLength(10);
        builder.Property(x => x.DescripcionIso4217).HasColumnName("DescripcionIso4217").HasMaxLength(50);

        builder.Property(x => x.UsuarioReg).HasColumnName("UsuarioReg").HasMaxLength(50).IsRequired();
        builder.Property(x => x.FechaReg).HasColumnName("FechaReg").IsRequired();
        builder.Property(x => x.UsuarioAct).HasColumnName("UsuarioAct").HasMaxLength(50);
        builder.Property(x => x.FechaAct).HasColumnName("FechaAct");

        builder.HasIndex(x => x.Codigo)
            .IsUnique()
            .HasDatabaseName("IdxTtMonedaCodigo");
    }
}
