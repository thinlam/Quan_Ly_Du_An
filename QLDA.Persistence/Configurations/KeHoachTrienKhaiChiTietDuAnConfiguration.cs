using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KeHoachTrienKhaiChiTietDuAnConfiguration : AggregateRootConfiguration<KeHoachTrienKhaiChiTietDuAn> {
    public override void Configure(EntityTypeBuilder<KeHoachTrienKhaiChiTietDuAn> builder) {
        builder.ToTable(nameof(KeHoachTrienKhaiChiTietDuAn));
        builder.ConfigureForBase();
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.Ten).HasColumnType("nvarchar(4000)");
        builder.Property(e => e.GhiChu).HasColumnType("nvarchar(4000)");

        builder.HasOne(e => e.TrangThaiXuLy)
          .WithMany()
          .HasForeignKey(e => e.TrangThaiId)
          .OnDelete(DeleteBehavior.Restrict)
          .IsRequired(false);

        builder.HasOne(e => e.DuAn)
            .WithMany()
            .HasForeignKey(e => e.DuAnId)
            .OnDelete(DeleteBehavior.Restrict); 

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb
            );
        
    }
}