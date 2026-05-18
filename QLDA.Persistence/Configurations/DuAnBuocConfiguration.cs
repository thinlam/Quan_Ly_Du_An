using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DuAnBuocConfiguration : AggregateRootConfiguration<DuAnBuoc> {
    public override void Configure(EntityTypeBuilder<DuAnBuoc> builder) {
        builder.ToTable(nameof(DuAnBuoc));

        builder.ConfigureForBase();

        builder.Property(e => e.NgayDuKienBatDau)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayDuKienKetThuc)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayThucTeBatDau)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.Property(e => e.NgayThucTeKetThuc)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb
            );

        builder.HasOne(e => e.Buoc)
            .WithMany(e => e.DuAnBuocs)
            .HasForeignKey(e => e.BuocId);
        
        
        builder.HasMany(e => e.VanBanQuyetDinhs)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
        
         
        builder.HasMany(e => e.BaoCaos)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
        
        
        builder.HasMany(e => e.DangTaiKeHoachLcntLenMangs)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.HopDongs)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
            
        builder.HasMany(e => e.KetQuaTrungThaus)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.PhuLucHopDongs)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.TamUngs)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(e => e.ThanhToans)
            .WithOne(e => e.DuAnBuoc)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.PhongPhuTrachChinhId)
            .HasColumnName("PhongPhuTrachChinhId");
    }
}