using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class StoryConfiguration : IEntityTypeConfiguration<Story>
{
    public void Configure(EntityTypeBuilder<Story> builder)
    {
        builder.ToTable("Stories");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(s => s.Slug)
            .IsRequired()
            .HasMaxLength(260);

        builder.Property(s => s.AlternativeTitle)
            .HasMaxLength(250);

        builder.Property(s => s.Description)
            .HasMaxLength(4000);

        builder.Property(s => s.CoverImageUrl)
            .HasMaxLength(500);

        builder.Property(s => s.AverageRating)
            .HasPrecision(3, 2)
            .HasDefaultValue(0);

        builder.Property(s => s.CreatedBy)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(s => s.RemovalReason)
            .HasMaxLength(500);

        builder.Property(s => s.RowVersion)
            .IsRowVersion();

        // Foreign Key to Author (Restrict delete)
        builder.HasOne(s => s.Author)
            .WithMany(a => a.Stories)
            .HasForeignKey(s => s.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Global Query Filter for Soft Delete
        builder.HasQueryFilter(s => !s.IsDeleted);

        // Indexes
        builder.HasIndex(s => s.Slug)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(s => new { s.PublishStatus, s.Status, s.IsDeleted, s.UpdatedAt });
        builder.HasIndex(s => new { s.IsDeleted, s.DeletedAt });
        builder.HasIndex(s => s.ViewCount);
        builder.HasIndex(s => s.AverageRating);
        builder.HasIndex(s => s.CreatedAt);
    }
}
