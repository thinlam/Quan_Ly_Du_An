using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class NguoiDungMacDinhTheoPhongConfiguration : AggregateRootConfiguration<NguoiDungMacDinhTheoPhong>
{
    public override void Configure(EntityTypeBuilder<NguoiDungMacDinhTheoPhong> builder)
    {
        builder.ToTable(nameof(NguoiDungMacDinhTheoPhong));
        builder.ConfigureForBase();

        builder.HasIndex(e => new { e.PhongBanId, e.NguoiDungId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
