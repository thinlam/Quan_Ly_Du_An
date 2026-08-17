using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ToTrinhThamDinhNhaThauConfiguration : AggregateRootConfiguration<ToTrinhThamDinhNhaThau> {
    public override void Configure(EntityTypeBuilder<ToTrinhThamDinhNhaThau> builder) {
        builder.ToTable(nameof(ToTrinhThamDinhNhaThau));
        builder.ConfigureForBase();
        builder.HasOne(e => e.DuAn)
        .WithMany()
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb 
            );  
        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        // Issue #179 — Tờ trình thẩm định nhà thầu (1 gói thầu / 1 nhà thầu)
        builder.HasOne(e => e.GoiThau)
            .WithMany()
            .HasForeignKey(e => e.GoiThauId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasOne(e => e.NhaThau)
            .WithMany()
            .HasForeignKey(e => e.NhaThauId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}