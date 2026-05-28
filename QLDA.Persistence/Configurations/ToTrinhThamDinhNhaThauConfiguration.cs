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
        builder.Property(x => x.So).HasMaxLength(200);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb => toDb.ToUniversalTime(),
                fromDb => fromDb
            );
    
        builder.Property(x => x.TrichYeu)
            .HasMaxLength(4000);
      

        builder.HasOne(e => e.TrangThai)
            .WithMany()
            .HasForeignKey(e => e.TrangThaiId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
    }
}