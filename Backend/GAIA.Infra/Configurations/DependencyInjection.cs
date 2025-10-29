using GAIA.Core.Interfaces.Assessment;
using GAIA.Core.Services.Assessment;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.DomainEvents;
using GAIA.Domain.Framework.Entities;
using GAIA.Domain.InsightContent.DomainEvents;
using GAIA.Domain.InsightContent.Entities;
using GAIA.Infra.Projections;
using Marten;
using Marten.Events.Projections;
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
        options.Schema.For<Framework>();
        options.Schema.For<FrameworkNode>();
        options.Schema.For<Assessment>();
        options.Schema.For<InsightContent>();

        // Enable event sourcing if needed
        options.Events.AddEventType(typeof(FrameworkCreated));
        options.Events.AddEventType(typeof(FrameworkNodeCreated));
        options.Events.AddEventType(typeof(AssessmentCreated));
        options.Events.AddEventType(typeof(InsightContentCreated));

        // Configure projections (Snapshot projections for aggregates)
        // Inline lifecycle to update document state within the same transaction      
        options.Projections.Snapshot<AssessmentProjection>(SnapshotLifecycle.Inline);

      }).UseLightweightSessions()
      .ApplyAllDatabaseChangesOnStartup();

      services.AddScoped<IAssessmentEventWriter, AssessmentEventWriter>();
      services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(AppDomain.CurrentDomain.Load("GAIA.Core")));
    }
  }
}
