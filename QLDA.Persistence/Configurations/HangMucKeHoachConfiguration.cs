using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class HangMucKeHoachConfiguration : AggregateRootConfiguration<HangMucKeHoach> {
    public override void Configure(EntityTypeBuilder<HangMucKeHoach> builder) {
        builder.ToTable(nameof(HangMucKeHoach));
        builder.ConfigureForBase();
        builder.HasOne(e => e.GiaiDoan)
        .WithMany()
        .HasForeignKey(e => e.GiaiDoanId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DonViChuTri)
        .WithMany()
        .HasForeignKey(e => e.DonViChuTriId)
        .OnDelete(DeleteBehavior.Restrict);


    }
}