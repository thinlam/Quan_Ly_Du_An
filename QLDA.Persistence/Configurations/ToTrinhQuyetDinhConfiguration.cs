using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class ChiDinhThauConfiguration : AggregateRootConfiguration<ToTrinhQuyetDinh> {
    public override void Configure(EntityTypeBuilder<ToTrinhQuyetDinh> builder)
    {

        builder.ToTable(nameof(ToTrinhQuyetDinh));    

        builder.ConfigureForBase();

        builder.Property(e => e.HoSoMoiThauId).IsRequired();

    }
}