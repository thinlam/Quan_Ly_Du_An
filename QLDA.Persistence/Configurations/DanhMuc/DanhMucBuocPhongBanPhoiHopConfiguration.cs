using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucBuocPhongBanPhoiHopConfiguration : AggregateRootConfiguration<DanhMucBuocPhongBanPhoiHop>
{
    public override void Configure(EntityTypeBuilder<DanhMucBuocPhongBanPhoiHop> builder)
    {
        builder.ToTable("DmBuocPhongBanPhoiHop");

        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("BuocId");
        builder.Property(e => e.RightId).HasColumnName("PhongBanId");

        builder.HasOne(e => e.Buoc)
            .WithMany(e => e.BuocPhongBanPhoiHops)
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
