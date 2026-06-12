using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DuongDiTrangThaiToTrinhConfiguration : AggregateRootConfiguration<DuongDiTrangThaiToTrinh> {
    public override void Configure(EntityTypeBuilder<DuongDiTrangThaiToTrinh> builder) {
        builder.ToTable(nameof(DuongDiTrangThaiToTrinh));

        builder.HasKey(e => new { e.Id });

    }
}
