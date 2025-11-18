using GAIA.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.EFCore.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
  public void Configure(EntityTypeBuilder<Document> entity)
  {
    entity.ToTable("Documents");

    entity.HasKey(e => e.Id);

    entity.Property(e => e.Name)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(e => e.Status)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.Category)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.CreatedAt)
        .IsRequired();

    entity.Property(e => e.UpdatedAt)
        .IsRequired(false);
  }
}
