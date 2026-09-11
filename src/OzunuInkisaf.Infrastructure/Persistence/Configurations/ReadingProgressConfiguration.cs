using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class ReadingProgressConfiguration : IEntityTypeConfiguration<ReadingProgress>
{
    public void Configure(EntityTypeBuilder<ReadingProgress> builder)
    {
        builder.ToTable("ReadingProgresses");

        builder.HasKey(p => p.Id);

        builder.HasIndex(p => new { p.UserId, p.BookId }).IsUnique();
    }
}
