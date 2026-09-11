using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OzunuInkisaf.Domain.Entities;

namespace OzunuInkisaf.Infrastructure.Persistence.Configurations;

public class KhatimCycleConfiguration : IEntityTypeConfiguration<KhatimCycle>
{
    public void Configure(EntityTypeBuilder<KhatimCycle> builder)
    {
        builder.ToTable("KhatimCycles");

        builder.HasKey(c => c.Id);

        builder.HasIndex(c => c.CycleNumber).IsUnique();

        builder.HasMany(c => c.JuzClaims)
            .WithOne(j => j.KhatimCycle)
            .HasForeignKey(j => j.KhatimCycleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JuzClaimConfiguration : IEntityTypeConfiguration<JuzClaim>
{
    public void Configure(EntityTypeBuilder<JuzClaim> builder)
    {
        builder.ToTable("JuzClaims");

        builder.HasKey(j => j.Id);

        builder.HasIndex(j => new { j.KhatimCycleId, j.JuzNumber }).IsUnique();

        builder.HasOne(j => j.User)
            .WithMany(u => u.JuzClaims)
            .HasForeignKey(j => j.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
