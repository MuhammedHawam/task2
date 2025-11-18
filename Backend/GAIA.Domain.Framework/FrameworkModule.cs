using GAIA.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GAIA.Domain.Framework;

public static class FrameworkModule
{
  public static void LoadFrameworkModule(this IServiceCollection services)
  {
    services.AddScoped<IFrameworkService, FrameworkService>();
  }
}
