using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucLoaiCongViecConfiguration : AggregateRootConfiguration<DanhMucLoaiCongViec> {
    public override void Configure(EntityTypeBuilder<DanhMucLoaiCongViec> builder) {
        builder.ToTable("DanhMucLoaiCongViec");
        builder.ConfigureForDanhMuc();

        builder.HasMany(e => e.GoiThaus)
            .WithOne(e => e.LoaiCongViec)
            .HasForeignKey(e => e.LoaiCongViecId)
            .OnDelete(DeleteBehavior.ClientSetNull);
    }
}