using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class PheDuyetDuToanConfiguration : IEntityTypeConfiguration<PheDuyetDuToan> {
    public void Configure(EntityTypeBuilder<PheDuyetDuToan> builder) {
        // TPT inheritance: derived type has its own table, key configured on base type (VanBanQuyetDinh)
        builder.ToTable(nameof(PheDuyetDuToan));

        // Configure derived properties only
        builder.Property(e => e.GiaTriDuThau).HasPrecision(18, 2);
        builder.Property(e=>e.TrangThaiId).HasDefaultValue(5); //Default : Migrated
        builder.HasOne(e => e.ChucVu)
            .WithMany(e => e.PheDuyetDuToans)
            .HasForeignKey(e => e.ChucVuId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrangThai)
            .WithMany(e => e.PheDuyetDuToans)
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Histories)
            .WithOne(e => e.PheDuyetDuToan)
            .HasForeignKey(e => e.PheDuyetDuToanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}