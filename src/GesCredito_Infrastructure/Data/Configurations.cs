using GesCredito.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GesCredito.Infrastructure.Data;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> b)
    {
        b.ToTable("Clientes");
        b.HasKey(x => x.Id);
        b.Property(x => x.Identificacion).HasMaxLength(20).IsRequired();
        b.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
        b.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
        b.Property(x => x.Email).HasMaxLength(150).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(30);
        b.Property(x => x.Direccion).HasMaxLength(200);
        b.Property(x => x.FechaRegistro).HasColumnType("datetime2");
        b.HasIndex(x => x.Identificacion).IsUnique().HasDatabaseName("UX_Clientes_Identificacion");
        b.Ignore(x => x.NombreCompleto);
    }
}

public class CreditoConfiguration : IEntityTypeConfiguration<Credito>
{
    public void Configure(EntityTypeBuilder<Credito> b)
    {
        b.ToTable("Creditos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Numero).HasMaxLength(30).IsRequired();
        b.Property(x => x.Monto).HasColumnType("decimal(18,2)");
        b.Property(x => x.TasaInteresAnual).HasColumnType("decimal(9,4)");
        b.Property(x => x.CuotaMensual).HasColumnType("decimal(18,2)");
        b.Property(x => x.Estado).HasConversion<byte>();
        b.Property(x => x.FechaSolicitud).HasColumnType("datetime2");
        b.Property(x => x.FechaDesembolso).HasColumnType("datetime2");
        b.Property(x => x.Observaciones).HasMaxLength(500);
        b.Ignore(x => x.TotalAPagar);

        b.HasIndex(x => x.Numero).IsUnique().HasDatabaseName("UX_Creditos_Numero");
        b.HasIndex(x => new { x.ClienteId, x.Estado }).HasDatabaseName("IX_Creditos_Cliente_Estado");

        b.HasOne(x => x.Cliente)
            .WithMany(c => c.Creditos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
