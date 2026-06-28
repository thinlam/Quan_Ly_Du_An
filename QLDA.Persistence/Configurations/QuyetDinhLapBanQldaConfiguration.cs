using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhLapBanQldaConfiguration : AggregateRootConfiguration<QuyetDinhLapBanQLDA> {
    public override void Configure(EntityTypeBuilder<QuyetDinhLapBanQLDA> builder) {
        builder.ToTable(nameof(QuyetDinhLapBanQLDA));
        builder.Property(x => x.SoDuThao)
            .HasMaxLength(4000);
        builder.Property(x => x.TrichYeuDuThao)
            .HasMaxLength(4000);


     builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

     
    }
}