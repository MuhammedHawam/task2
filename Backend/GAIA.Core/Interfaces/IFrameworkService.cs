using GAIA.Core.DTOs;

namespace GAIA.Core.Interfaces;

public interface IFrameworkService
{
  Task<IEnumerable<FrameworkConfigurationOption>> ListFrameworksWithOptions();
}
