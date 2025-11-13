using GAIA.Domain.Documents.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.Configurations;

public class DocumentEntityTypeConfiguration : IEntityTypeConfiguration<Document>
{
  public void Configure(EntityTypeBuilder<Document> builder)
  {
    builder.ToTable("Documents");

    builder.HasKey(document => document.Id);

    builder.Property(document => document.Content)
      .IsRequired();

    builder.Property(document => document.Status)
      .HasMaxLength(100)
      .IsRequired();

    builder.Property(document => document.Category)
      .HasMaxLength(100)
      .IsRequired();

    builder.Property(document => document.Name)
      .HasMaxLength(255)
      .IsRequired();
  }
}
