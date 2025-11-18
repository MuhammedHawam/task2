using GAIA.Domain.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GAIA.Infra.EFCore.Configurations;

public class FrameworkConfiguration : IEntityTypeConfiguration<Framework>
{
  public void Configure(EntityTypeBuilder<Framework> entity)
  {
    entity.ToTable("Frameworks");

    entity.HasKey(e => e.Id);

    entity.Property(e => e.Title)
        .IsRequired()
        .HasMaxLength(200);

    entity.Property(e => e.Description)
        .IsRequired()
        .HasMaxLength(2000);

    entity.Property(e => e.CreatedBy)
        .IsRequired()
        .HasMaxLength(100);

    entity.Property(e => e.CreatedAt)
        .IsRequired();

    // ---- Owned Type: Root Node ----
    entity.OwnsOne(e => e.Root, root =>
    {
      root.Property(n => n.Id)
          .IsRequired();

      root.Property(n => n.Content)
          .HasMaxLength(5000);

      // ---- Nested Owned Collection: Children ----
      root.OwnsMany(n => n.Children, child =>
      {
        child.Property(c => c.Id)
            .IsRequired();

        child.Property(c => c.Content)
            .HasMaxLength(5000);

        child.WithOwner()
            .HasForeignKey("RootId");

        child.ToTable("FrameworkRootChildren");  // optional: name table explicitly
      });

      root.ToTable("FrameworkRoots"); // optional: naming for clarity
    });

    // ---- One-to-many: Depths ----
    entity.HasMany(e => e.Depths)
        .WithOne()
        .HasForeignKey("FrameworkId")
        .OnDelete(DeleteBehavior.Cascade);

    // ---- One-to-many: Scorings ----
    entity.HasMany(e => e.Scorings)
        .WithOne()
        .HasForeignKey("FrameworkId")
        .OnDelete(DeleteBehavior.Cascade);
  }
}
