using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ChiDinhThauConfiguration : AggregateRootConfiguration<ToTrinhQuyetDinh> {
    public override void Configure(EntityTypeBuilder<ToTrinhQuyetDinh> builder)
    {

        builder.ToTable(nameof(ToTrinhQuyetDinh));    

        builder.ConfigureForBase();

        builder.HasOne<HoSoMoiThauDienTu>()
        .WithOne(e => e.ToTrinh)
        .HasForeignKey<ToTrinhQuyetDinh>(e => e.HoSoMoiThauToTrinhId);

        builder.HasOne<HoSoMoiThauDienTu>()
            .WithOne(e => e.QuyetDinh)
            .HasForeignKey<ToTrinhQuyetDinh>(e => e.HoSoMoiThauQuyetDinhId);
    }
}