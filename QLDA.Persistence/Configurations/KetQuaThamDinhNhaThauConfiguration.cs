using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KetQuaThamDinhNhaThauConfiguration : AggregateRootConfiguration<KetQuaThamDinhNhaThau> {
    public override void Configure(EntityTypeBuilder<KetQuaThamDinhNhaThau> builder) {
        builder.ToTable(nameof(KetQuaThamDinhNhaThau));

        builder.HasKey(e => new { e.ToTrinhId, e.NhaThauId });

        builder.Property(e => e.ToTrinhId).HasColumnName("ToTrinhId");
        builder.Property(e => e.NhaThauId).HasColumnName("NhaThauId");
        builder.Property(e => e.KetQuaDanhGia).HasColumnType("nvarchar(max)");
        builder.HasOne(e => e.ToTrinhThamDinhNhaThau)
            .WithMany(e => e.NhaThaus)
            .HasForeignKey(e => e.ToTrinhId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
