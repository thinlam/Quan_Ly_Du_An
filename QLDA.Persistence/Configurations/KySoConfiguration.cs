using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class KySoConfiguration : AggregateRootConfiguration<KySo> {
    public override void Configure(EntityTypeBuilder<KySo> builder) {
        builder.ToTable("KySo");
        builder.ConfigureForBase();
        
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.SerialChungThu).HasMaxLength(200);
        builder.Property(e => e.ToChucCap).HasMaxLength(500);
        builder.Property(e => e.PhamVi).HasConversion<int>();

        builder.Property(e => e.HieuLucTu)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.Property(e => e.HieuLucDen)
            .HasConversion(
                toDb => toDb.HasValue ? toDb.Value.ToUniversalTime() : (DateTimeOffset?)null,
                fromDb => fromDb);

        builder.HasOne(e => e.PhuongThucKySo)
            .WithMany(e => e.KySos)
            .HasForeignKey(e => e.PhuongThucKySoId)
            .OnDelete(DeleteBehavior.SetNull);
        
        builder.HasOne(e => e.ChucVu)
            .WithMany()
            .HasForeignKey(e => e.ChucVuId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}