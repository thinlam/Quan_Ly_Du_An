using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DanhMucPhuongXaConfiguration : MasterRootConfiguration<DmPhuongXa> {
    public override void Configure(EntityTypeBuilder<DmPhuongXa> builder) {
        
        builder
            .HasNoKey()
            .ToTable("DM_PHUONGXA");

        builder.Property(e => e.MaPhuongXa).HasMaxLength(5);
        builder.Property(e => e.MoTa).HasMaxLength(4000);
        builder.Property(e => e.Id).HasColumnName("PhuongXaID");
        builder.Property(e => e.QuanHuyenId).HasColumnName("QuanHuyenID");
        builder.Property(e => e.TenPhuongXa).HasMaxLength(500);
    }
}