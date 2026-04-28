using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucPhuongThucKySoConfiguration : AggregateRootConfiguration<DanhMucPhuongThucKySo> {
    public override void Configure(EntityTypeBuilder<DanhMucPhuongThucKySo> builder) {
        builder.ToTable("DmPhuongThucKySo");
        builder.ConfigureForDanhMuc();
        builder.HasMany(e => e.KySos)
            .WithOne(e => e.PhuongThucKySo)
            .HasForeignKey(e => e.PhuongThucKySoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}