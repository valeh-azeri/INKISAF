using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class TallyTemplateConfiguration : IEntityTypeConfiguration<TallyTemplate>
{
    public void Configure(EntityTypeBuilder<TallyTemplate> builder)
    {
        builder.ToTable("TallyTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title).IsRequired().HasMaxLength(200);

        builder.HasOne(t => t.CreatedBy)
            .WithMany()
            .HasForeignKey(t => t.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(t => t.Items)
            .WithOne(i => i.TallyTemplate)
            .HasForeignKey(i => i.TallyTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TallyItemConfiguration : IEntityTypeConfiguration<TallyItem>
{
    public void Configure(EntityTypeBuilder<TallyItem> builder)
    {
        builder.ToTable("TallyItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Title).IsRequired().HasMaxLength(300);
        builder.Property(i => i.SpecificDaysCsv).HasMaxLength(50);

        builder.HasMany(i => i.Entries)
            .WithOne(e => e.TallyItem)
            .HasForeignKey(e => e.TallyItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class TallyEntryConfiguration : IEntityTypeConfiguration<TallyEntry>
{
    public void Configure(EntityTypeBuilder<TallyEntry> builder)
    {
        builder.ToTable("TallyEntries");

        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.TallyItemId, e.UserId, e.Date }).IsUnique();

        builder.HasOne(e => e.User)
            .WithMany(u => u.TallyEntries)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
