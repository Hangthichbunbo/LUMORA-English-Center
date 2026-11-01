using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class FollowedStoryConfiguration : IEntityTypeConfiguration<FollowedStory>
{
    public void Configure(EntityTypeBuilder<FollowedStory> builder)
    {
        builder.ToTable("FollowedStories");

        builder.HasKey(fs => new { fs.UserId, fs.StoryId });

        builder.HasOne(fs => fs.User)
            .WithMany(u => u.FollowedStories)
            .HasForeignKey(fs => fs.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(fs => fs.Story)
            .WithMany(s => s.FollowedStories)
            .HasForeignKey(fs => fs.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(fs => fs.CreatedAt);
    }
}
