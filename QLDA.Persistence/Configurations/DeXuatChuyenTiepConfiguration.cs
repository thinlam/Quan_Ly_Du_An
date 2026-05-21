using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class DeXuatChuyenTiepConfiguration : AggregateRootConfiguration<DeXuatChuyenTiep> {
    public override void Configure(EntityTypeBuilder<DeXuatChuyenTiep> builder)
    {

        builder.ToTable(nameof(DeXuatChuyenTiep));    

        builder.ConfigureForBase();

        builder.Property(x => x.KhoiLuongThucTe).HasMaxLength(4000);
        builder.Property(x => x.KhoiLuongDuKien).HasMaxLength(4000);

        builder.Property(e => e.SoLieuGiaiNgan).HasPrecision(18, 2);
        builder.Property(e => e.UocGiaiNgan).HasPrecision(18, 2);
        builder.Property(e => e.NhuCauKinhPhi).HasPrecision(18, 2);

      
        builder.Property(e => e.TrangThaiId);
      
        builder.HasOne(e => e.DuAn)
        .WithMany()
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb 
            );
        
        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}