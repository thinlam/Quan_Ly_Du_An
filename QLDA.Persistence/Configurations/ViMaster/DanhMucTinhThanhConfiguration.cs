using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DanhMucTinhThanhConfiguration : MasterRootConfiguration<DmTinhThanh> {
    public override void Configure(EntityTypeBuilder<DmTinhThanh> builder) {
        
        builder
            .HasNoKey()
            .ToTable("DM_TINHTHANH");

        builder.Property(e => e.MaTinhThanh).HasMaxLength(5);
        builder.Property(e => e.MoTa).HasMaxLength(4000);
        builder.Property(e => e.QuocGiaId).HasColumnName("QuocGiaID");
        builder.Property(e => e.TenTinhThanh).HasMaxLength(500);
        builder.Property(e => e.Id).HasColumnName("TinhThanhID");
    }
}