using GAIA.Core.Interfaces;
using GAIA.Domain.Framework;
using GAIA.Infra.EFCore.Repositories;
using GAIA.Infra.EFCore.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GAIA.Infra.EFCore;

public static class DependencyInjection
{
  public static void AddInfraEfCore(this IServiceCollection services, IConfigurationSection connectionConf)
  {
    var connectString = connectionConf["ConnectionString"];
    var schema = connectionConf["Schema"] ?? "ef-core";

    services.AddScoped<GaiaDbContext>(provider =>
    {
      var options = provider.GetRequiredService<DbContextOptions<GaiaDbContext>>();
      return new GaiaDbContext(options, schema);
    });

    services.AddDbContext<GaiaDbContext>(options
      => options
        .UseNpgsql(connectString)
        .UseSeeding((ctx, _) => FrameworksSeeder.Seed(ctx))
        .UseAsyncSeeding(async (ctx, _, cToken) => await FrameworksSeeder.SeedAsync(ctx, cToken)));

    services.AddScoped<IRepository<Framework>, FrameworkRepository>();
  }

  public static async Task MigrateDatabase(this IServiceProvider serviceProvider)
  {
    using var scope = serviceProvider.CreateScope();
    var dbCtx = scope.ServiceProvider.GetRequiredService<GaiaDbContext>();
    await dbCtx.Database.MigrateAsync();
  }
}
