using GAIA.Domain.FileStorage.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.EFCore.Configurations;

public class StoredFileConfiguration : IEntityTypeConfiguration<StoredFile>
{
  public void Configure(EntityTypeBuilder<StoredFile> builder)
  {
    builder.ToTable("StoredFiles");

    builder.HasKey(file => file.Id);

    builder.Property(file => file.FileName)
      .IsRequired()
      .HasMaxLength(260);

    builder.Property(file => file.ContentType)
      .IsRequired()
      .HasMaxLength(256);

    builder.Property(file => file.Category)
      .IsRequired()
      .HasMaxLength(128);

    builder.Property(file => file.Description)
      .HasMaxLength(512);

    builder.Property(file => file.ContextType)
      .HasMaxLength(128);

    builder.Property(file => file.SizeInBytes)
      .IsRequired();

    builder.Property(file => file.Content)
      .IsRequired();

    builder.Property(file => file.CreatedAt)
      .IsRequired();

    builder.Property(file => file.CreatedBy)
      .IsRequired();

    builder.HasIndex(file => file.ContextId);
  }
}
