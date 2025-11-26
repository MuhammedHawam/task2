using GAIA.Core.Assessment.Interfaces;
using GAIA.Core.Services.Assessment;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.InsightContent.DomainEvents;
using GAIA.Domain.InsightContent.Entities;
using GAIA.Infra.Projections;
using GAIA.Infra.Repositories;
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
        options.Schema.For<AssessmentDetails>();
        options.Schema.For<InsightContent>();
        options.Schema.For<AssessmentDepth>()
                  .UniqueIndex(depth => depth.FrameworkId, depth => depth.Depth);
        options.Schema.For<AssessmentScoring>();
        options.Schema.For<AssessmentUserAssignment>();
        options.Events.AddEventType(typeof(AssessmentDetailsCreated));
        options.Events.AddEventType(typeof(AssessmentCreated));
        options.Events.AddEventType(typeof(AssessmentUpdated));
        options.Events.AddEventType(typeof(InsightContentCreated));

        // Configure projections for the AssessmentDetails aggregate using a SingleStream projection
        // Inline lifecycle updates document state within the same transaction
        options.Projections.Add<AssessmentDetailsProjection>(ProjectionLifecycle.Inline);
        options.Projections.Add<AssessmentProjection>(ProjectionLifecycle.Inline);
      }).UseLightweightSessions()
      .ApplyAllDatabaseChangesOnStartup();

      services.AddScoped<IAssessmentDetailsEventWriter, AssessmentDetailsEventWriter>();
      services.AddHostedService<AssessmentConfigurationSeedService>();
      services.AddScoped<IAssessmentEventWriter, AssessmentEventWriter>();
      services.AddScoped<IAssessmentRepository, AssessmentRepository>();
      services.AddScoped<IAssessmentUserAssignmentRepository, AssessmentUserAssignmentRepository>();
      services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(AppDomain.CurrentDomain.Load("GAIA.Core")));
    }
  }
}
