using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("Ratings", t =>
        {
            t.HasCheckConstraint("CK_Ratings_Score_Range", "[Score] >= 1 AND [Score] <= 5");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.HasOne(r => r.Story)
            .WithMany(s => s.Ratings)
            .HasForeignKey(r => r.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Crucial Unique Index: 1 user can only rate 1 story once
        builder.HasIndex(r => new { r.StoryId, r.UserId })
            .IsUnique();
    }
}
