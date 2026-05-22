using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class CanBoConfiguration : MasterRootConfiguration<CanBo> {
    public override void Configure(EntityTypeBuilder<CanBo> builder) {
        builder.HasKey(e => e.Id);

        builder.ToTable("CANBO");

        builder.Property(e => e.Id).HasColumnName("CanBoID");
        builder.Property(e => e.ChucDanhId).HasColumnName("ChucDanhID");
        builder.Property(e => e.ChuyenMon).HasMaxLength(2000);
        builder.Property(e => e.ConNguoiId).HasColumnName("ConNguoiID");
        builder.Property(e => e.MaSoCanBo).HasMaxLength(20);
        builder.Property(e => e.NgheNghiep).HasMaxLength(2000);
        builder.Property(e => e.SoBhxh)
            .HasMaxLength(20)
            .HasColumnName("SoBHXH");

        // builder.HasOne(d => d.ChucDanh).WithMany(p => p.Canbos)
        //     .HasForeignKey(d => d.ChucDanhId);
        //
        // builder.HasOne(d => d.ConNguoi).WithMany(p => p.Canbos)
        //     .HasForeignKey(d => d.ConNguoiId);
    }
}