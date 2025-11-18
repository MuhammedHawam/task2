using GAIA.Domain.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.EFCore.Configurations
{
  public class ScoringConfiguration : IEntityTypeConfiguration<Scoring>
  {
    public void Configure(EntityTypeBuilder<Scoring> entity)
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.Description).HasMaxLength(500);
    }
  }
}
