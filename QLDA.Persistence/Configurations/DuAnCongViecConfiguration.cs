using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DuAnCongViecConfiguration : IEntityTypeConfiguration<DuAnCongViec> {
    public void Configure(EntityTypeBuilder<DuAnCongViec> builder) {
        builder.ToTable(nameof(DuAnCongViec));

        // Composite key
        builder.HasKey(e => new { e.LeftId, e.RightId });
        builder.Property(e => e.LeftId).HasColumnName("DuAnId");
        builder.Property(e => e.RightId).HasColumnName("CongViecId");
        // Relationships
        builder.HasOne(e => e.DuAn)
            .WithMany(e => e.DuAnCongViecs)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(e => e.IsDeleted)
            .HasDefaultValue(false);

        builder.Property(e => e.NguoiPhuTrachChinhId)
            .IsRequired(false);

        builder.Property(e => e.NguoiTaoId)
            .IsRequired(false);
    }
}