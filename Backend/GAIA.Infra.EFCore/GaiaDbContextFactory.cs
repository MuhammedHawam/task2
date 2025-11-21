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
    // Keep the design-time connection string aligned with the default runtime settings
    // so migrations CLI tooling doesn't fall back to the obsolete `gaia` role.
    optionsBuilder.UseNpgsql("Host=localhost;Database=gaia;Username=postgres;Password=postgres");

    return new GaiaDbContext(optionsBuilder.Options, "ef-core");
  }
}
