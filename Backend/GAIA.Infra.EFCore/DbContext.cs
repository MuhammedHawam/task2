using GAIA.Domain.FileStorage.Entities;
using GAIA.Domain.Framework;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra.EFCore;

public class GaiaDbContext(DbContextOptions<GaiaDbContext> options, string schema) : DbContext(options)
{
  public DbSet<Framework> Frameworks { get; set; }
  public DbSet<StoredFile> StoredFiles { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.HasDefaultSchema(schema);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(GaiaDbContext).Assembly);
  }
}
