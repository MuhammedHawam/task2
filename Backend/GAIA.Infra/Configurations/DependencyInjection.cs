using GAIA.Core.Configuration.Interfaces;
using GAIA.Core.Interfaces.Assessment;
using GAIA.Core.Services.Assessment;
using GAIA.Core.Services.Configuration;
using GAIA.Domain.Assessment.DomainEvents;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.DomainEvents;
using GAIA.Domain.Framework.Entities;
using GAIA.Domain.InsightContent.DomainEvents;
using GAIA.Domain.InsightContent.Entities;
using GAIA.Infra.Projections;
using GAIA.Infra.SeedData;
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
        options.Schema.For<AssessmentDepth>();
        options.Schema.For<AssessmentScoring>();
        options.Schema.For<InsightContent>();

        // Enable event sourcing if needed
        options.Events.AddEventType(typeof(FrameworkCreated));
        options.Events.AddEventType(typeof(FrameworkNodeCreated));
        options.Events.AddEventType(typeof(AssessmentCreated));
        options.Events.AddEventType(typeof(InsightContentCreated));

        // Configure projections for the Assessment aggregate using a SingleStream projection
        // Inline lifecycle updates document state within the same transaction
        options.Projections.Add<AssessmentProjection>(ProjectionLifecycle.Inline);

        }).UseLightweightSessions()
          .ApplyAllDatabaseChangesOnStartup();

        services.AddScoped<IAssessmentConfigurationDataSource, MartenAssessmentConfigurationDataSource>();
        services.AddScoped<IAssessmentConfigurationService, AssessmentConfigurationService>();
        services.AddScoped<IAssessmentEventWriter, AssessmentEventWriter>();
        services.AddHostedService<AssessmentConfigurationSeedService>();
        services.AddMediatR(cfg =>
          cfg.RegisterServicesFromAssembly(AppDomain.CurrentDomain.Load("GAIA.Core")));
    }
  }
}
