using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucPhuongAnThietKeConfiguration : AggregateRootConfiguration<DanhMucPhuongAnThietKe> {
    public override void Configure(EntityTypeBuilder<DanhMucPhuongAnThietKe> builder) {
        builder.ToTable("DmPhuongAnThietKe");
        builder.ConfigureForDanhMuc();
        
    }
}
