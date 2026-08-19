using MicroServicioQRs.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace MicroServicioQRs.Repository
{
    public class QrDbContext : DbContext
    {
        public QrDbContext(DbContextOptions<QrDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuarios vive en el schema Carnet_Identity_User (NO Carnet_Core_User)
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios", schema: "Carnet_Identity_User");
                entity.HasKey(e => e.UsuarioID);
                entity.Property(e => e.UsuarioID).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Identificacion).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.FechaModificacion).HasDefaultValueSql("GETUTCDATE()");
            });

            // Bitacoras: schema por confirmar. Lo dejo en Carnet_Identity_User;
            // si tu tabla esta en otro schema, ajusta esta linea.
            modelBuilder.Entity<Bitacora>(entity =>
            {
                entity.ToTable("Bitacoras", schema: "Carnet_Identity_User");
                entity.HasKey(e => e.BitacoraID);
                entity.Property(e => e.BitacoraID).HasDefaultValueSql("NEWID()");
                entity.Property(e => e.Fecha).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.Usuario).HasMaxLength(255);
            });
        }
    }
}