using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDuyetKHLCNTConfiguration : AggregateRootConfiguration<QuyetDinhDuyetKHLCNT> {
    public override void Configure(EntityTypeBuilder<QuyetDinhDuyetKHLCNT> builder) {
        builder.ToTable(nameof(QuyetDinhDuyetKHLCNT));

        builder.HasOne(e => e.KeHoachLuaChonNhaThau)
            .WithOne(e => e.QuyetDinhDuyetKHLCNT)
            .HasForeignKey<QuyetDinhDuyetKHLCNT>(e => e.KeHoachLuaChonNhaThauId);

        builder.HasOne(e => e.VanBanQuyetDinh)
       .WithMany()
       .HasForeignKey(e => e.QuyetDinhId)
       .OnDelete(DeleteBehavior.Restrict);


    }
}