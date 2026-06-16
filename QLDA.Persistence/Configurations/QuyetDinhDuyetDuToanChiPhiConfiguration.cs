using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class QuyetDinhDuToanChiPhiConfiguration : AggregateRootConfiguration<QuyetDinhDuyetDuToanChiPhi>
{
    public override void Configure(EntityTypeBuilder<QuyetDinhDuyetDuToanChiPhi> builder)
    {
        builder.ToTable(nameof(QuyetDinhDuyetDuToanChiPhi));
        builder.HasKey(e => new {e.Id, e.QuyetDinhDuToanId });

        builder.Property(e => e.GiaTri)
            .HasPrecision(18, 2) 
            .IsRequired();

        builder.Property(e => e.ChiPhi)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(e => e.GiaTri)
            .HasPrecision(18, 2) 
            .IsRequired();

        builder.HasOne(e => e.QuyetDinhDuToan)
            .WithMany(p => p.ChiPhis)
            .HasForeignKey(e => e.QuyetDinhDuToanId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}
