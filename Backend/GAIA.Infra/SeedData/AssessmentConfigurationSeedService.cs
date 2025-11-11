using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GAIA.Domain.Assessment.Entities;
using GAIA.Domain.Framework.Entities;
using Marten;
using Marten.Linq;
using Microsoft.Extensions.Hosting;

namespace GAIA.Infra.SeedData
{
  public class AssessmentConfigurationSeedService : IHostedService
  {
    private readonly IDocumentStore _store;

    public AssessmentConfigurationSeedService(IDocumentStore store)
    {
      _store = store;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      using var session = _store.LightweightSession();

      if (await session.Query<Framework>().AnyAsync(cancellationToken))
      {
        return;
      }

      var now = DateTime.UtcNow;

      var frameworks = new List<Framework>
      {
        new Framework
        {
          Id = SeedIds.FrameworkCyberSecurity,
          Title = "Cybersecurity Maturity",
          Description = "Baseline cybersecurity controls grouped by maturity level.",
          CreatedAt = now,
          CreatedBy = SeedIds.SeedUser,
          Root = new FrameworkNode
          {
            Id = SeedIds.FrameworkCyberSecurityRoot,
            Content = "Cybersecurity Root",
            Depth = 0,
            Children = new List<FrameworkNode>()
          }
        },
        new Framework
        {
          Id = SeedIds.FrameworkPrivacy,
          Title = "Privacy Compliance",
          Description = "Privacy compliance considerations across assessment depths.",
          CreatedAt = now,
          CreatedBy = SeedIds.SeedUser,
          Root = new FrameworkNode
          {
            Id = SeedIds.FrameworkPrivacyRoot,
            Content = "Privacy Root",
            Depth = 0,
            Children = new List<FrameworkNode>()
          }
        }
      };

      var depths = new List<AssessmentDepth>
      {
        new AssessmentDepth
        {
          Id = SeedIds.DepthCyberSecurityInitial,
          FrameworkId = SeedIds.FrameworkCyberSecurity,
          Depth = 1,
          Name = "Initial",
          Description = "Introductory security capabilities focused on core hygiene."
        },
        new AssessmentDepth
        {
          Id = SeedIds.DepthCyberSecurityAdvanced,
          FrameworkId = SeedIds.FrameworkCyberSecurity,
          Depth = 2,
          Name = "Advanced",
          Description = "Advanced security practices emphasizing continuous improvement."
        },
        new AssessmentDepth
        {
          Id = SeedIds.DepthPrivacyBaseline,
          FrameworkId = SeedIds.FrameworkPrivacy,
          Depth = 1,
          Name = "Baseline",
          Description = "Baseline privacy safeguards for regulated data."
        },
        new AssessmentDepth
        {
          Id = SeedIds.DepthPrivacyEnhanced,
          FrameworkId = SeedIds.FrameworkPrivacy,
          Depth = 2,
          Name = "Enhanced",
          Description = "Enhanced privacy controls for data minimization and subject rights."
        }
      };

      var scorings = new List<AssessmentScoring>
      {
        new AssessmentScoring
        {
          Id = SeedIds.ScoringCyberSecurityBronze,
          AssessmentDepthId = SeedIds.DepthCyberSecurityInitial,
          Name = "Bronze",
          Description = "Minimum viable implementation of foundational controls."
        },
        new AssessmentScoring
        {
          Id = SeedIds.ScoringCyberSecurityGold,
          AssessmentDepthId = SeedIds.DepthCyberSecurityInitial,
          Name = "Gold",
          Description = "Established and repeatable cybersecurity practices."
        },
        new AssessmentScoring
        {
          Id = SeedIds.ScoringCyberSecurityPlatinum,
          AssessmentDepthId = SeedIds.DepthCyberSecurityAdvanced,
          Name = "Platinum",
          Description = "Optimized security program with measurable outcomes."
        },
        new AssessmentScoring
        {
          Id = SeedIds.ScoringPrivacySilver,
          AssessmentDepthId = SeedIds.DepthPrivacyBaseline,
          Name = "Silver",
          Description = "Foundational privacy controls aligned to regulatory expectations."
        },
        new AssessmentScoring
        {
          Id = SeedIds.ScoringPrivacyGold,
          AssessmentDepthId = SeedIds.DepthPrivacyBaseline,
          Name = "Gold",
          Description = "Proactive privacy guardrails supported by automation."
        },
        new AssessmentScoring
        {
          Id = SeedIds.ScoringPrivacyPlatinum,
          AssessmentDepthId = SeedIds.DepthPrivacyEnhanced,
          Name = "Platinum",
          Description = "Privacy-by-design embedded across product lifecycle."
        }
      };

      session.Store(frameworks);
      session.Store(depths);
      session.Store(scorings);

      await session.SaveChangesAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static class SeedIds
    {
      public static readonly Guid SeedUser = Guid.Parse("8f6c9ea9-56cd-4a47-93c1-7ec9ac299d8c");

      public static readonly Guid FrameworkCyberSecurity = Guid.Parse("ad2c63f9-b856-4e36-91f9-5da7b42e1f33");
      public static readonly Guid FrameworkCyberSecurityRoot = Guid.Parse("97f1c6f9-ec45-44f9-a2c6-3aceb9af9f1a");
      public static readonly Guid FrameworkPrivacy = Guid.Parse("e39f4b6b-82af-4f63-8805-2df0c06ef3c9");
      public static readonly Guid FrameworkPrivacyRoot = Guid.Parse("00bc5404-1d37-4aba-9dfd-bc1aac81593b");

      public static readonly Guid DepthCyberSecurityInitial = Guid.Parse("982b6a76-a312-46c5-9a9d-34175e5b56dd");
      public static readonly Guid DepthCyberSecurityAdvanced = Guid.Parse("3e39f295-30ab-40e6-a38f-b5b5c0d2358f");
      public static readonly Guid DepthPrivacyBaseline = Guid.Parse("4ff22e8a-9db1-4b56-b486-d94cf1d54f56");
      public static readonly Guid DepthPrivacyEnhanced = Guid.Parse("2dc6c607-9cc4-4b51-8d0f-7a60ba9a2cbe");

      public static readonly Guid ScoringCyberSecurityBronze = Guid.Parse("d758ff3c-fb19-4a88-bb29-9a48b204361e");
      public static readonly Guid ScoringCyberSecurityGold = Guid.Parse("7b6907ca-d0d5-4df9-8436-3a369f9e67e0");
      public static readonly Guid ScoringCyberSecurityPlatinum = Guid.Parse("6c493922-61ca-4f13-9b31-44cd5df590be");
      public static readonly Guid ScoringPrivacySilver = Guid.Parse("fa45e5f2-38a9-4b1e-a544-ec45e63f1fd2");
      public static readonly Guid ScoringPrivacyGold = Guid.Parse("4a30e8ed-3a32-4d4f-b3fc-cf6a5668a698");
      public static readonly Guid ScoringPrivacyPlatinum = Guid.Parse("57eb0f2f-95bb-4c66-8510-64e05849722b");
    }
  }
}
