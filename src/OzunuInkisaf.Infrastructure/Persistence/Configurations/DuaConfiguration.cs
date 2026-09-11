using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class DuaConfiguration : IEntityTypeConfiguration<Dua>
{
    public void Configure(EntityTypeBuilder<Dua> builder)
    {
        builder.ToTable("Duas");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).IsRequired().HasMaxLength(200);
        builder.Property(d => d.ArabicText).HasMaxLength(4000);
        builder.Property(d => d.Translation).HasMaxLength(4000);
        builder.Property(d => d.Transliteration).HasMaxLength(4000);
        builder.Property(d => d.PdfPath).HasMaxLength(500);

        builder.HasOne(d => d.CreatedBy)
            .WithMany()
            .HasForeignKey(d => d.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
