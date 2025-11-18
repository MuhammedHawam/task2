using GAIA.Domain.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.EFCore.Configurations
{
  public class FrameworkDepthConfiguration : IEntityTypeConfiguration<FrameworkDepth>
  {
    public void Configure(EntityTypeBuilder<FrameworkDepth> entity)
    {
      entity.HasKey(e => e.Id);
      entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
      entity.Property(e => e.Depth).IsRequired();
      entity.HasIndex(e => new { e.Depth, e.Name });
    }
  }
}
