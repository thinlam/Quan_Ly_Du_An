using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ThoaThuanGiaoViecConfiguration : AggregateRootConfiguration<ThoaThuanGiaoViec> {
    public override void Configure(EntityTypeBuilder<ThoaThuanGiaoViec> builder) {
        builder.ToTable(nameof(ThoaThuanGiaoViec));


        builder.Property(e => e.GoiThauId).HasColumnName("GoiThauId");
        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.Restrict);

       
        builder.Property(e => e.PhamVi).HasColumnType("nvarchar(4000)");
        builder.Property(e => e.ChatLuong).HasColumnType("nvarchar(4000)");
        builder.Property(e => e.NoiDung).HasColumnType("nvarchar(4000)");


        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}
