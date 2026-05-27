using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class ThuyetMinhDuAnConfiguration : AggregateRootConfiguration<ThuyetMinhDuAn> {
    public override void Configure(EntityTypeBuilder<ThuyetMinhDuAn> builder) {
        builder.ToTable(nameof(ThuyetMinhDuAn));
        builder.ConfigureForBase();

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.ToUniversalTime(),
                fromDb => fromDb
            );

        builder.Property(e => e.So)
            .HasMaxLength(100);

        builder.Property(e => e.TrichYeu)
            .HasMaxLength(500);
        builder.Property(e => e.KetQuaThamDinh)
          .HasMaxLength(4000);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.TrangThaiThamDinh)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiThamDinhId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}