using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class StoryCategoryConfiguration : IEntityTypeConfiguration<StoryCategory>
{
    public void Configure(EntityTypeBuilder<StoryCategory> builder)
    {
        builder.ToTable("StoryCategories");

        builder.HasKey(sc => new { sc.StoryId, sc.CategoryId });

        builder.HasOne(sc => sc.Story)
            .WithMany(s => s.StoryCategories)
            .HasForeignKey(sc => sc.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sc => sc.Category)
            .WithMany(c => c.StoryCategories)
            .HasForeignKey(sc => sc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
