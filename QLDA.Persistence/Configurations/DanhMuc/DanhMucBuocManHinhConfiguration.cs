using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations.DanhMuc;

public class DanhMucBuocManHinhConfiguration : AggregateRootConfiguration<DanhMucBuocManHinh> {
    public override void Configure(EntityTypeBuilder<DanhMucBuocManHinh> builder) {
        builder.ToTable("DmBuocManHinh");
        builder.HasKey(e => new { e.BuocId, e.ManHinhId });

        builder.Property(e => e.Stt)
            .HasDefaultValue(0)
            .IsRequired();

        builder.HasOne(e => e.Buoc)
            .WithMany(e => e.BuocManHinhs)
            .HasForeignKey(e => e.BuocId)
            .OnDelete(DeleteBehavior.Cascade);

    }
}