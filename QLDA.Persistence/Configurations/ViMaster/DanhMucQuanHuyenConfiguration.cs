using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DanhMucQuanHuyenConfiguration : MasterRootConfiguration<DmQuanHuyen> {
    public override void Configure(EntityTypeBuilder<DmQuanHuyen> builder) {
        builder
            .HasNoKey()
            .ToTable("DM_QUANHUYEN");

        builder.Property(e => e.MaQuanHuyen).HasMaxLength(5);
        builder.Property(e => e.MoTa).HasMaxLength(4000);
        builder.Property(e => e.Id).HasColumnName("QuanHuyenID");
        builder.Property(e => e.TenQuanHuyen).HasMaxLength(500);
        builder.Property(e => e.TinhThanhId).HasColumnName("TinhThanhID");
    }
}