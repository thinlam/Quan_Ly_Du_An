using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;

namespace QLDA.Persistence.Configurations;

public class KeHoachLuaChonNhaThauRutGonConfiguration : AggregateRootConfiguration<KeHoachLuaChonNhaThauRutGon> {
    public override void Configure(EntityTypeBuilder<KeHoachLuaChonNhaThauRutGon> builder) {
        builder.ToTable(nameof(KeHoachLuaChonNhaThauRutGon));


        builder.Property(e => e.GoiThauId).HasColumnName("GoiThauId");
        builder.Property(e => e.NhaThauId).HasColumnName("NhaThauId");
        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.NhaThau)
            .WithMany()
            .HasForeignKey(e => e.NhaThauId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.KetQuaDanhGia).HasColumnType("nvarchar(4000)");



        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
