using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDuToanNguonVonConfiguration : AggregateRootConfiguration<QuyetDinhDuyetDuToanNguonVon>
{
    public override void Configure(EntityTypeBuilder<QuyetDinhDuyetDuToanNguonVon> builder)
    {
        builder.ToTable(nameof(QuyetDinhDuyetDuToanNguonVon));

        builder.HasKey(e => new { e.QuyetDinhDuToanId, e.Id });
        builder.Property(e => e.GiaTri)
            .HasPrecision(18, 2) 
            .IsRequired();

        builder.Property(e => e.Nam)
            .IsRequired(false);

        // 4. Cấu hình mối quan hệ (Relationships)
        // Quan hệ 1-Nhiều với bảng cha QuyetDinhDuToan
        builder.HasOne(e => e.QuyetDinhDuToan)
            .WithMany(p => p.KeHoachVons)
            .HasForeignKey(e => e.QuyetDinhDuToanId)
            .OnDelete(DeleteBehavior.Cascade); 

    }
}
