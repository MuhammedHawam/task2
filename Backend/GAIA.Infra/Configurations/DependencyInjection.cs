using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.Services.Assessment;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.InsightContent.DomainEvents;
using GAIA.Domain.InsightContent.Entities;
using GAIA.Infra.Projections;
using GAIA.Infra.SeedDataService;
using JasperFx.Events.Projections;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace GAIA.Infra.Configurations
{
  public static class DependencyInjection
  {
    public static void AddInfrastructure(this IServiceCollection services, string connectionString)
    {
      services.AddMarten(options =>
      {
        // Set the connection string for PostgreSQL
        options.Connection(connectionString);
        options.DatabaseSchemaName = "marten";
        // Register entities for Marten
        options.Schema.For<Assessment>();
        options.Schema.For<InsightContent>();
        options.Schema.For<AssessmentDepth>()
                  .UniqueIndex(depth => depth.FrameworkId, depth => depth.Depth);
        options.Schema.For<AssessmentScoring>();
        options.Events.AddEventType(typeof(AssessmentCreated));
        options.Events.AddEventType(typeof(AssessmentEvidenceDocumentAdded));
        options.Events.AddEventType(typeof(InsightContentCreated));

        // Configure projections for the Assessment aggregate using a SingleStream projection
        // Inline lifecycle updates document state within the same transaction
        options.Projections.Add<AssessmentProjection>(ProjectionLifecycle.Inline);

      }).UseLightweightSessions()
      .ApplyAllDatabaseChangesOnStartup();

      services.AddScoped<IAssessmentEventWriter, AssessmentEventWriter>();
      services.AddHostedService<AssessmentConfigurationSeedService>();
      services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(AppDomain.CurrentDomain.Load("GAIA.Core")));
    }
  }
}
