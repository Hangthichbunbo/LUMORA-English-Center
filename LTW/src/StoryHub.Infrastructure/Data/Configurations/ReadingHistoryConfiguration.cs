using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class ReadingHistoryConfiguration : IEntityTypeConfiguration<ReadingHistory>
{
    public void Configure(EntityTypeBuilder<ReadingHistory> builder)
    {
        builder.ToTable("ReadingHistories");

        builder.HasKey(rh => rh.Id);

        builder.Property(rh => rh.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(rh => rh.Progress)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        builder.HasOne(rh => rh.User)
            .WithMany(u => u.ReadingHistories)
            .HasForeignKey(rh => rh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rh => rh.Story)
            .WithMany(s => s.ReadingHistories)
            .HasForeignKey(rh => rh.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rh => rh.Chapter)
            .WithMany(c => c.ReadingHistories)
            .HasForeignKey(rh => rh.ChapterId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique index: 1 user has 1 active reading history per story
        builder.HasIndex(rh => new { rh.UserId, rh.StoryId })
            .IsUnique();

        builder.HasIndex(rh => rh.LastReadAt);
    }
}
