using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class ToTrinhPheDuyetConfiguration : AggregateRootConfiguration<ToTrinhPheDuyet> {
    public override void Configure(EntityTypeBuilder<ToTrinhPheDuyet> builder) {
        builder.ToTable(nameof(ToTrinhPheDuyet));
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
        builder.Property(x => x.Ten).HasMaxLength(4000);
        builder.Property(x => x.So).HasMaxLength(200);
        builder.Property(x => x.Loai).HasMaxLength(100);

        builder.Property(e => e.NgayToTrinh)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
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