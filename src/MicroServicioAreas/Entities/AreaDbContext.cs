using Microsoft.EntityFrameworkCore;

namespace MicroservicioAreas.Entities
{
    public class AreaDbContext : DbContext
    {
        public AreaDbContext(DbContextOptions<AreaDbContext> options) : base(options) { }

        public DbSet<Area> Areas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Area>(entity =>
            {
                entity.ToTable("AreasTrabajo", schema: "Carnet_Core_User");

                entity.HasKey(e => e.AreaTrabID);

                entity.Property(e => e.AreaTrabID)
                    .HasColumnName("AreaTrabID")
                    .HasDefaultValueSql("NEWID()");

                entity.Property(e => e.NombreAreaTrab)
                    .IsRequired()
                    .HasMaxLength(200);

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