using MicroServicioQRs.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroServicioQRs.Repository
{
    public class QrDbContext : DbContext
    {
        public QrDbContext(DbContextOptions<QrDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<EmailXUsuario> EmailXUsuarios { get; set; }
        public DbSet<Bitacora> Bitacoras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            modelBuilder.Entity<EmailXUsuario>(entity =>
            {
                entity.ToTable("EmailXUsuarios", schema: "Carnet_Identity_User");
                entity.HasKey(e => e.Email);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            });

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