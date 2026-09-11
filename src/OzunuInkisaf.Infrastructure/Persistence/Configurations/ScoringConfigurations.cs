using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class PointsTransactionConfiguration : IEntityTypeConfiguration<PointsTransaction>
{
    public void Configure(EntityTypeBuilder<PointsTransaction> builder)
    {
        builder.ToTable("PointsTransactions");

        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithMany(u => u.PointsTransactions)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.Source, p.SourceId });
    }
}

public class WinnerAwardConfiguration : IEntityTypeConfiguration<WinnerAward>
{
    public void Configure(EntityTypeBuilder<WinnerAward> builder)
    {
        builder.ToTable("WinnerAwards");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.CategoriesCsv).HasMaxLength(100);

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.AnnouncedBy)
            .WithMany()
            .HasForeignKey(a => a.AnnouncedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
