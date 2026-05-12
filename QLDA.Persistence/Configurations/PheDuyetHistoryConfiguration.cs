using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class PheDuyetHistoryConfiguration : AggregateRootConfiguration<PheDuyetHistory> {
    public override void Configure(EntityTypeBuilder<PheDuyetHistory> builder) {
        builder.ToTable(nameof(PheDuyetHistory));
        builder.ConfigureForBase();

        // Composite index for polymorphic lookup
        builder.HasIndex(e => new { e.EntityName, e.EntityId });

        // No FK constraint on EntityId (polymorphic)
        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
