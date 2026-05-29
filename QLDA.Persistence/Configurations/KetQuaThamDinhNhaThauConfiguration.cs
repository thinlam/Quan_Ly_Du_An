using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KetQuaThamDinhNhaThauConfiguration : AggregateRootConfiguration<KetQuaThamDinhNhaThau> {
    public override void Configure(EntityTypeBuilder<KetQuaThamDinhNhaThau> builder) {
        builder.ToTable(nameof(KetQuaThamDinhNhaThau));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.ToTrinhId).HasColumnName("ToTrinhId");
        builder.Property(e => e.NhaThauId).HasColumnName("NhaThauId");
        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.Restrict);
        // 3. Mối quan hệ Khóa ngoại 1: Đã xóa phần trùng lặp ở cuối
        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.Restrict);


        builder.Property(e => e.KetQuaDanhGia).HasColumnType("nvarchar(max)");
        builder.HasOne(e => e.ToTrinhThamDinhNhaThau)
            .WithMany(e => e.NhaThaus)
            .HasForeignKey(e => e.ToTrinhId)
            .OnDelete(DeleteBehavior.Cascade);
     

    }
}
