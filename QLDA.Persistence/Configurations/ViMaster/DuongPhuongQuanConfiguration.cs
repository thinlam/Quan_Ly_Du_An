using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DuongPhuongQuanConfiguration : MasterRootConfiguration<DuongPhuongQuan> {
    public override void Configure(EntityTypeBuilder<DuongPhuongQuan> builder) {
        builder.HasNoKey().ToTable("DUONG_PHUONG_QUAN");

        builder.Property(e => e.DuongId).HasColumnName("DuongID");
        builder.Property(e => e.Id).HasColumnName("DuongPhuongQuanID");
        builder.Property(e => e.PhuongXaId).HasColumnName("PhuongXaID");
        builder.Property(e => e.QuanHuyenId).HasColumnName("QuanHuyenID");
    }
}