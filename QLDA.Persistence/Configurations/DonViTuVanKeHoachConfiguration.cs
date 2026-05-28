using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class DonViTuVanKeHoachConfiguration : AggregateRootConfiguration<DonViTuVanKeHoach> {
    public override void Configure(EntityTypeBuilder<DonViTuVanKeHoach> builder) {
        builder.ToTable(nameof(DonViTuVanKeHoach));
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).UseIdentityColumn();

        builder.HasKey(e => new { e.KeHoachId });
        builder.Property(e => e.KeHoachId).HasColumnName("KeHoachLCNTId");
        builder.Property(e => e.TenDonVi).HasColumnType("nvarchar(400)");
        builder.HasOne(e => e.KeHoach)
            .WithMany(e => e.DonViTuVans)
            .HasForeignKey(e => e.KeHoachId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}