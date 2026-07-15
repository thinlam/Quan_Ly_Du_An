using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class TrienKhaiKeHoachLCNTConfiguration : AggregateRootConfiguration<TrienKhaiKeHoachLCNT> {
    public override void Configure(EntityTypeBuilder<TrienKhaiKeHoachLCNT> builder) {
        builder.ToTable(nameof(TrienKhaiKeHoachLCNT));
        builder.ConfigureForBase();
        builder.HasOne(e => e.DuAn)
        .WithMany()
        .HasForeignKey(e => e.DuAnId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.GoiThau)
        .WithMany()
        .HasForeignKey(e => e.GoiThauId)
        .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.DmHinhThucLCNT)
      .WithMany()
      .HasForeignKey(e => e.HinhThucLCNT)
      .OnDelete(DeleteBehavior.Restrict); 
        
        builder.Property(e => e.BuocId)
            .HasConversion(
                toDb => toDb == 0 ? null : toDb,
                fromDb => fromDb 
            );
        builder.Property(x => x.So).HasMaxLength(200);

        builder.Property(e => e.NgayTrinh)
            .HasConversion(
                toDb =>  toDb.ToUniversalTime() ,
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