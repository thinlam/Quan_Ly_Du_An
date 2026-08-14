using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class HoSoMoiThauDienTuConfiguration : AggregateRootConfiguration<HoSoMoiThauDienTu>
{
    public override void Configure(EntityTypeBuilder<HoSoMoiThauDienTu> builder)
    {
        builder.ToTable("HoSoMoiThauDienTu");

        builder.ConfigureForBase();

        builder.Property(e => e.ThoiGianThucHien).HasMaxLength(200);


        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Buoc)
            .WithMany()
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.HinhThucLuaChonNhaThau)
            .WithMany()
            .HasForeignKey(e => e.HinhThucLuaChonNhaThauId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TrangThaiPheDuyet)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.SetNull);

        // ToTrinh/QuyetDinh (ToTrinhQuyetDinh) không còn là navigation EF 1-1 qua FK riêng —
        // dùng chung bảng qua EntityId + Loai (Issue #179), load thủ công ở Command/Query,
        // xem HoSoMoiThauDienTuMappings/HoSoMoiThauDienTuUpdateCommand/HoSoMoiThauDienTuDuyetCommand.
    }
}