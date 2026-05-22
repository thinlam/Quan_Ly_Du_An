using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class DanhMucCapDonViConfiguration : MasterRootConfiguration<DanhMucCapDonVi> {
    public override void Configure(EntityTypeBuilder<DanhMucCapDonVi> builder) {

        builder.HasKey(e => e.Id);

        builder.ToTable("E_CAPDONVI");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("CapDonViID");
        builder.Property(e => e.MoTa).HasMaxLength(4000);
        builder.Property(e => e.TenCapDonVi).HasMaxLength(500);
    }
}