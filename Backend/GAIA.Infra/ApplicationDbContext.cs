using GAIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Document> Documents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Document>(builder =>
        {
            builder.ToTable("Documents");
            builder.HasKey(document => document.Id);

            builder.Property(document => document.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(document => document.Category)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(document => document.Status)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(document => document.Content)
                .IsRequired();
        });
    }
}
