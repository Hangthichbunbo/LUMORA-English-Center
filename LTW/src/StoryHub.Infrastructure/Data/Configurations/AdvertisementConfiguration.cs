using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class AdvertisementConfiguration : IEntityTypeConfiguration<Advertisement>
{
    public void Configure(EntityTypeBuilder<Advertisement> builder)
    {
        builder.ToTable("Advertisements");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(500);

        builder.Property(a => a.DestinationUrl)
            .HasMaxLength(1000);

        builder.Property(a => a.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(a => a.TargetCategorySlug)
            .HasMaxLength(100);

        builder.Property(a => a.RowVersion)
            .IsRowVersion();

        builder.HasIndex(a => new { a.Placement, a.IsEnabled, a.StartAt, a.EndAt, a.Priority });
    }
}
