using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class GoiThauTrinhPheDuyetKetQuaConfiguration : AggregateRootConfiguration<GoiThauTrinhPheDuyetKetQua> {
    public override void Configure(EntityTypeBuilder<GoiThauTrinhPheDuyetKetQua> builder) {
        builder.ToTable(nameof(GoiThauTrinhPheDuyetKetQua));

        builder.HasKey(e => new { e.ToTrinhId, e.GoiThauId });

        builder.Property(e => e.ToTrinhId).HasColumnName("ToTrinhId");
        builder.Property(e => e.GoiThauId).HasColumnName("GoiThauId");

        builder.HasOne(e => e.ToTrinhKetQuaGoiThau)
            .WithMany(e => e.GoiThaus)
            .HasForeignKey(e => e.ToTrinhId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
