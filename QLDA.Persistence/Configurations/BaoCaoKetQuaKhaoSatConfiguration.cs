using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class BaoCaoKetQuaKhaoSatConfiguration : AggregateRootConfiguration<BaoCaoKetQuaKhaoSat>
{
    public override void Configure(EntityTypeBuilder<BaoCaoKetQuaKhaoSat> builder)
    {
        builder.ToTable(nameof(BaoCaoKetQuaKhaoSat));
        builder.ConfigureForBase();

        builder.Property(e => e.NoiDungBaoCao).HasMaxLength(4000);
        builder.Property(e => e.NoiDungNghiemThu).HasMaxLength(4000);

        builder.Property(e => e.NgayKhaoSat)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
