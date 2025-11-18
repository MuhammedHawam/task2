using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GAIA.Infra.EFCore;

public class GaiaDbContextFactory : IDesignTimeDbContextFactory<GaiaDbContext>
{
  public GaiaDbContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<GaiaDbContext>();

    // Use a placeholder connection string for migrations CLI tooling.
    // This will be replaced at runtime with the actual connection string.
    optionsBuilder.UseNpgsql("Host=localhost;Database=gaia;Username=gaia;Password=not_secure");

    return new GaiaDbContext(optionsBuilder.Options, "ef-core");
  }
}
