using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ToTrinhThamDinhBuocXuLyConfiguration : AggregateRootConfiguration<ToTrinhThamDinhBuocXuLy> {
    public override void Configure(EntityTypeBuilder<ToTrinhThamDinhBuocXuLy> builder) {
        builder.ToTable(nameof(ToTrinhThamDinhBuocXuLy));
        builder.ConfigureForBase();

        builder.Property(e => e.So).HasMaxLength(200);
        builder.Property(e => e.NoiDung).HasColumnType("nvarchar(max)");

        builder.HasOne(e => e.ToTrinh)
            .WithMany(e => e.BuocXuLys)
            .HasForeignKey(e => e.ToTrinhId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ToTrinhId, e.Loai });
    }
}
