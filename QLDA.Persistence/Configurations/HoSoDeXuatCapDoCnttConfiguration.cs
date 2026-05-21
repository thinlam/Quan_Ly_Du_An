using System.Collections;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class HoSoDeXuatCapDoCnttConfiguration : AggregateRootConfiguration<HoSoDeXuatCapDoCntt>
{
    public override void Configure(EntityTypeBuilder<HoSoDeXuatCapDoCntt> builder)
    {
        builder.ToTable(nameof(HoSoDeXuatCapDoCntt));
        builder.ConfigureForBase();
        builder.Property(e => e.NoiDungDeNghi).HasMaxLength(1000);
        builder.Property(e => e.NoiDungBaoCao).HasMaxLength(2000);
        builder.Property(e => e.NoiDungDuThao).HasMaxLength(2000);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.HasOne(e => e.CapDo)
            .WithMany(e => e.HoSoDeXuatCapDoCntts)
            .HasForeignKey(e => e.CapDoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}