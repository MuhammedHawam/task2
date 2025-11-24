using System.Reflection;
using System.Text.Json;
using GAIA.Domain.Framework;
using Microsoft.EntityFrameworkCore;

namespace GAIA.Infra.EFCore.Seeders;

public record FrameworkJsonData
{
  public required string Title { get; init; }
  public required string Description { get; init; }
  public required DateTime CreatedAt { get; init; }
  public required string CreatedBy { get; init; }
  public required FrameworkNodeJsonData Root { get; init; }
}

public record FrameworkNodeJsonData
{
  public string? Content { get; init; }
  public ICollection<FrameworkNodeJsonData> Children { get; init; } = new List<FrameworkNodeJsonData>();
}

public static class FrameworksSeeder
{
  private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
  {
    PropertyNameCaseInsensitive = true,
  };

  public static void Seed(DbContext ctx)
  {
    SeedCore(ctx);
  }

  public static Task SeedAsync(DbContext ctx, CancellationToken cToken = default)
  {
    SeedCore(ctx);
    return Task.CompletedTask;
  }

  private static void SeedCore(DbContext ctx)
  {
    if (ctx.Set<Framework>().Any())
    {
      return;
    }

    ReadEmbeddedResources("GAIA.Infra.EFCore._frameworks.").ToList().ForEach(json =>
    {
      var frameworkData = JsonSerializer.Deserialize<FrameworkJsonData>(json, JsonOptions);
      if (frameworkData == null)
      {
        throw new InvalidOperationException("Failed to deserialize framework data.");
      }

      ctx.Set<Framework>().Add(MapToFrameworkEntity(frameworkData));
    });

    ctx.SaveChanges();
  }


  private static IEnumerable<string> ReadEmbeddedResources(string prefix)
  {
    var assembly = Assembly.GetExecutingAssembly();
    var resourceNames = assembly.GetManifestResourceNames().ToList().FindAll(name => name.StartsWith(prefix));

    return resourceNames.Select(resourceName =>
    {
      using var stream = assembly.GetManifestResourceStream(resourceName);
      if (stream == null)
      {
        throw new FileLoadException($"Embedded resource '{resourceName}' could not be read");
      }

      using var reader = new StreamReader(stream);
      return reader.ReadToEnd();
    });
  }

  private static Framework MapToFrameworkEntity(FrameworkJsonData data)
  {
    return new Framework
    {
      Id = Guid.Parse("73c9c20b-fbe4-4ae3-afd7-7621aad8bc4e"),
      Title = data.Title,
      Description = data.Description,
      CreatedAt = data.CreatedAt,
      CreatedBy = data.CreatedBy,
      Root = MapToFrameworkNode(data.Root),
      Depths = new List<FrameworkDepth>(),
      Scorings = new List<Scoring>(),
    };
  }

  private static FrameworkNode MapToFrameworkNode(FrameworkNodeJsonData data)
  {
    return new FrameworkNode
    {
      Id = Guid.Parse("d837d7e0-257b-400d-8f17-b62e6d12487c"),
      Content = data.Content,
      Children = data.Children.Select(MapToFrameworkNode).ToList(),
    };
  }
}
