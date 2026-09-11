using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Title).IsRequired().HasMaxLength(300);
        builder.Property(b => b.Author).HasMaxLength(300);
        builder.Property(b => b.FilePath).IsRequired().HasMaxLength(1000);
        builder.Property(b => b.AssignedMonthLabel).HasMaxLength(100);

        builder.HasOne(b => b.UploadedBy)
            .WithMany()
            .HasForeignKey(b => b.UploadedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.ReadingProgresses)
            .WithOne(p => p.Book)
            .HasForeignKey(p => p.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
