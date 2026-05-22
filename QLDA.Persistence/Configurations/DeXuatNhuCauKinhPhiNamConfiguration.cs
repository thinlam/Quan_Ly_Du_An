using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class DeXuatNhuCauKinhPhiNamConfiguration : AggregateRootConfiguration<DeXuatNhuCauKinhPhiNam>
{
    public override void Configure(EntityTypeBuilder<DeXuatNhuCauKinhPhiNam> builder)
    {

        builder.ToTable(nameof(DeXuatNhuCauKinhPhiNam));

        builder.ConfigureForBase();
        builder.Property(e => e.NgayKeHoach)
         .HasConversion(
             toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
             fromDb => fromDb
         );

        builder.Property(x => x.So).HasMaxLength(100);
        builder.Property(x => x.TrichYeu).HasMaxLength(2000);
        builder.Property(x => x.GhiChu).HasMaxLength(4000);
        builder.Property(e => e.TongKinhPhiDeXuat).HasPrecision(18, 2);
        builder.Property(e => e.TrangThaiId);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}