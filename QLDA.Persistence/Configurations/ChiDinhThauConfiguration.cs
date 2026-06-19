using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class ChiDinhThauConfiguration : AggregateRootConfiguration<ChiDinhThau> {
    public override void Configure(EntityTypeBuilder<ChiDinhThau> builder)
    {

        builder.ToTable(nameof(ChiDinhThau));    

        builder.ConfigureForBase();

        builder.Property(e => e.HoSoMoiThauId).IsRequired();

    }
}