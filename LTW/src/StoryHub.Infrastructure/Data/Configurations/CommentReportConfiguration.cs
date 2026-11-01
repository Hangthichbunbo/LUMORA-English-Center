using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class CommentReportConfiguration : IEntityTypeConfiguration<CommentReport>
{
    public void Configure(EntityTypeBuilder<CommentReport> builder)
    {
        builder.ToTable("CommentReports");

        builder.HasKey(cr => cr.Id);

        builder.Property(cr => cr.Reason)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(cr => cr.Description)
            .HasMaxLength(500);

        builder.Property(cr => cr.ReportedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(cr => cr.ReviewedByUserId)
            .HasMaxLength(450);

        builder.HasOne(cr => cr.Comment)
            .WithMany(c => c.Reports)
            .HasForeignKey(cr => cr.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cr => cr.ReportedByUser)
            .WithMany(u => u.Reports)
            .HasForeignKey(cr => cr.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cr => cr.ReviewedByUser)
            .WithMany()
            .HasForeignKey(cr => cr.ReviewedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cr => new { cr.Status, cr.CreatedAt });
    }
}
