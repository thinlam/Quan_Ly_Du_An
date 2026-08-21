using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDuyetKHLCNTConfiguration : AggregateRootConfiguration<QuyetDinhDuyetKHLCNT> {
    public override void Configure(EntityTypeBuilder<QuyetDinhDuyetKHLCNT> builder) {
        builder.ToTable(nameof(QuyetDinhDuyetKHLCNT));

        builder.HasOne(e => e.KeHoachLuaChonNhaThau)
            .WithOne(e => e.QuyetDinhDuyetKHLCNT)
            .HasForeignKey<QuyetDinhDuyetKHLCNT>(e => e.KeHoachLuaChonNhaThauId);

        // 1 kế hoạch chỉ 1 QĐ duyệt đang active; bản soft-delete không chiếm unique
        builder.HasIndex(e => e.KeHoachLuaChonNhaThauId)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(e => e.VanBanQuyetDinh)
       .WithMany()
       .HasForeignKey(e => e.Id)
       .OnDelete(DeleteBehavior.Restrict);

    }
}