using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class DeXuatChuTruongMoiConfiguration : AggregateRootConfiguration<DeXuatChuTruongMoi> {
    public override void Configure(EntityTypeBuilder<DeXuatChuTruongMoi> builder)
    {

        builder.ToTable(nameof(DeXuatChuTruongMoi));    

        builder.ConfigureForBase();
        builder.Property(e => e.NgayBatDauDuKien)
          .HasConversion(
              toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
              fromDb => fromDb
          );

        builder.Property(x => x.TomTatNoiDung).HasMaxLength(4000);

        builder.Property(e => e.TongMucDauTu).HasPrecision(18, 2);

        builder.Property(e => e.LanhDaoPhuTrachId);

        builder.Property(e => e.DonViPhuTrachChinhId);
      
        builder.Property(e => e.TrangThaiId);

        builder.HasOne(e => e.HinhThucDauTu)
            .WithMany()
            .HasForeignKey(e => e.HinhThucDauTuId)
            .OnDelete(DeleteBehavior.SetNull);

      
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