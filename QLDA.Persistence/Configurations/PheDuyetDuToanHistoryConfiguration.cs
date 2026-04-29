using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class PheDuyetDuToanHistoryConfiguration : AggregateRootConfiguration<PheDuyetDuToanHistory> {
    public override void Configure(EntityTypeBuilder<PheDuyetDuToanHistory> builder) {
        builder.ToTable(nameof(PheDuyetDuToanHistory));
        builder.ConfigureForBase();

        builder.HasOne(e => e.PheDuyetDuToan)
            .WithMany(e => e.Histories)
            .HasForeignKey(e => e.PheDuyetDuToanId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrangThaiAction)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiActionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}