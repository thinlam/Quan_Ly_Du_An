using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DuAnBuocPhongBanPhoiHopConfiguration : AggregateRootConfiguration<DuAnBuocPhongBanPhoiHop> {
    public override void Configure(EntityTypeBuilder<DuAnBuocPhongBanPhoiHop> builder) {
        builder.ToTable(nameof(DuAnBuocPhongBanPhoiHop));

        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("DuAnBuocId");
        builder.Property(e => e.RightId).HasColumnName("PhongBanId");

        builder.HasOne<DuAnBuoc>()
            .WithMany(e => e.DuAnBuocPhongBanPhoiHops)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);

        // builder.HasOne<DmDonVi>()
        //     .WithMany()
        //     .HasForeignKey(e => e.RightId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}