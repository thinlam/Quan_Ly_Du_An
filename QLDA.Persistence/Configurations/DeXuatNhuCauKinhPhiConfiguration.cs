using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class DeXuatNhuCauKinhPhiConfiguration : AggregateRootConfiguration<DeXuatNhuCauKinhPhi>
{
    public override void Configure(EntityTypeBuilder<DeXuatNhuCauKinhPhi> builder)
    {

        builder.ToTable(nameof(DeXuatNhuCauKinhPhi));

        builder.ConfigureForBase();
        builder.Property(e => e.NgayPhieuChuyen)
         .HasConversion(
             toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
             fromDb => fromDb
         );

        builder.Property(x => x.SoPhieuChuyen).HasMaxLength(100);
        builder.Property(x => x.TrichYeu).HasMaxLength(2000);

        builder.Property(e => e.TrangThaiId);
        builder.Property(e => e.DonViDeXuatId);

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