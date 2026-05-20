using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DeXuatDonViXuLyConfiguration : AggregateRootConfiguration<DeXuatDonViXuLy> {
    public override void Configure(EntityTypeBuilder<DeXuatDonViXuLy> builder) {
        builder.ToTable(nameof(DeXuatDonViXuLy));
        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("DuAnId");
        builder.Property(e => e.RightId).HasColumnName("DonViId");

        builder.HasOne(e => e.DeXuat)
            .WithMany(e => e.DeXuatDonViXuLys)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
