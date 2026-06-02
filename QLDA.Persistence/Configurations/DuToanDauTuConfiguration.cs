using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;

namespace QLDA.Persistence.Configurations;

public class DuToanDauTuConfiguration : AggregateRootConfiguration<DuToanDauTu> {
    public override void Configure(EntityTypeBuilder<DuToanDauTu> builder) {
        builder.ToTable(nameof(DuToanDauTu));
        builder.ConfigureForBase();
        builder.HasOne(e => e.DuAn)
        .WithMany()
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb 
            );
        builder.Property(x => x.SoToTrinh).HasMaxLength(30);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );
    
        builder.Property(x => x.TrichYeu)
            .HasMaxLength(4000);

        builder.HasOne(e => e.PhuongAnThietKe)
            .WithMany()
            .HasForeignKey(e => e.PhuongAnThietKeId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.NguonVon)
            .WithMany()
            .HasForeignKey(e => e.NguonVonId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}