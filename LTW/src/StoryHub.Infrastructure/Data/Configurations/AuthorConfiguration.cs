using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoryHub.Domain.Entities;

namespace StoryHub.Infrastructure.Data.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.ToTable("Authors");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(a => a.Slug)
            .IsRequired()
            .HasMaxLength(160);

        builder.Property(a => a.Biography)
            .HasMaxLength(3000);

        builder.HasQueryFilter(a => !a.IsDeleted);

        builder.HasIndex(a => a.Slug)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
