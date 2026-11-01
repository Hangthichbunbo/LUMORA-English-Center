using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable("Chapters");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(c => c.Slug)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(c => c.ChapterNumber)
            .HasPrecision(6, 2)
            .IsRequired();

        builder.Property(c => c.Content)
            .IsRequired();

        builder.Property(c => c.RowVersion)
            .IsRowVersion();

        // Foreign Key to Story
        builder.HasOne(c => c.Story)
            .WithMany(s => s.Chapters)
            .HasForeignKey(c => c.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(c => !c.IsDeleted);

        // Unique Indexes
        builder.HasIndex(c => new { c.StoryId, c.ChapterNumber })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => new { c.StoryId, c.Slug })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(c => new { c.StoryId, c.PublishStatus, c.ChapterNumber });
        builder.HasIndex(c => new { c.IsDeleted, c.DeletedAt });
    }
}
