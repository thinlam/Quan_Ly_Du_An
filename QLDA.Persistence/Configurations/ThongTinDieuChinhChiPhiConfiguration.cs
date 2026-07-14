using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ThongTinDieuChinhChiPhiConfiguration : AggregateRootConfiguration<ThongTinDieuChinhChiPhi> {
    public override void Configure(EntityTypeBuilder<ThongTinDieuChinhChiPhi> builder) {
        builder.ToTable(nameof(ThongTinDieuChinhChiPhi));

        builder.Property(e => e.TongMucDauTu).HasPrecision(18, 2);
        builder.Property(e => e.ChiPhiXayLap).HasPrecision(18, 2);
        builder.Property(e => e.ChiPhiThietBi).HasPrecision(18, 2);
        builder.Property(e => e.ChiPhiKhac).HasPrecision(18, 2);
        builder.Property(e => e.ChiPhiDuPhong).HasPrecision(18, 2);
    }
}