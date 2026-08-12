using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ToTrinhQuyetDinhConfiguration : AggregateRootConfiguration<ToTrinhQuyetDinh> {
    public override void Configure(EntityTypeBuilder<ToTrinhQuyetDinh> builder)
    {

        builder.ToTable(nameof(ToTrinhQuyetDinh));    

        builder.ConfigureForBase();

        // Dùng chung nhiều nghiệp vụ qua EntityId + Loai thay vì FK riêng (Issue #179).
        builder.HasIndex(e => new { e.EntityId, e.Loai });
    }
}