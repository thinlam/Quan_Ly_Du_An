using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DanhMucVaiTroChucVuConfiguration : MasterRootConfiguration<DanhMucVaiTroChucVu> {
    public override void Configure(EntityTypeBuilder<DanhMucVaiTroChucVu> builder) {

        builder.HasKey(e => e.Id);

        builder.ToTable("E_VAITROCHUCVU");
        builder.Property(e => e.Id).HasColumnName("VaiTro");
        builder.Property(e => e.ChucVu).HasMaxLength(50);
        builder.Property(e => e.MoTa).HasMaxLength(255);
    }
}