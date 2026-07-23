using MicroservicioTiposUsuario.Entities;
using Microsoft.EntityFrameworkCore;

namespace MicroServicioTiposUsuario.Repository
{
    public class TipoUsuarioDbContext : DbContext
    {
        public TipoUsuarioDbContext(DbContextOptions<TipoUsuarioDbContext> options) : base(options) { }

        public DbSet<TipoUsuario> TiposUsuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TipoUsuario>(entity =>
            {
                entity.ToTable("TiposUsuarios", schema: "Carnet_Identity_User");

                entity.HasKey(e => e.TipoUsuarioID);

                entity.Property(e => e.TipoUsuarioID)
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.NombreTipoUsuario)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.FechaModificacion)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.Estado)
                    .HasDefaultValue(true);
            });
        }
    }
}