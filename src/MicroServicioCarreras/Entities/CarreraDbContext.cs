using Microsoft.EntityFrameworkCore;

namespace MicroservicioCarreras.Entities
{
    public class CarreraDbContext : DbContext
    {
        public CarreraDbContext(DbContextOptions<CarreraDbContext> options) : base(options) { }

        public DbSet<Carrera> Carreras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Carrera>(entity =>
            {
                entity.ToTable("Carreras", schema: "Carnet_Core_User");

                entity.HasKey(e => e.CarreraID);

                entity.Property(e => e.CarreraID)
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.NombreCarrera)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.DirectorCarrera)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20);

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