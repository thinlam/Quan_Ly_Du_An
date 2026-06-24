using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KeHoachLuaChonNhaThauConfiguration : AggregateRootConfiguration<KeHoachLuaChonNhaThau> {
    public override void Configure(EntityTypeBuilder<KeHoachLuaChonNhaThau> builder) {
        builder.ToTable(nameof(KeHoachLuaChonNhaThau));
        builder.Property(e => e.LoaiKeHoach)
            .HasColumnType("varchar(500)");
           
    }
}