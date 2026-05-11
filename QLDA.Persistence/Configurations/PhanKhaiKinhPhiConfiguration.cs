using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class PhanKhaiKinhPhiConfiguration : IEntityTypeConfiguration<PhanKhaiKinhPhi> {
    public void Configure(EntityTypeBuilder<PhanKhaiKinhPhi> builder) {
        builder.ToTable(nameof(PhanKhaiKinhPhi));

        builder.Property(e => e.SoToTrinh).HasMaxLength(100);
        builder.Property(e => e.KinhPhiDeXuat).HasPrecision(18, 2);
        builder.Property(e => e.KinhPhiPhanKhai).HasPrecision(18, 2);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

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
