using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BuildingBlocks.Domain.Entities;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations.ViMaster;

public class CanBoDonViConfiguration : MasterRootConfiguration<CanBoDonVi> {
    public override void Configure(EntityTypeBuilder<CanBoDonVi> builder) {
        builder.HasKey(e => e.Id);
        builder.ToTable("CANBO_DONVI");

        builder.Property(e => e.Id).HasColumnName("CanBoDonViID");
        builder.Property(e => e.CanBoId).HasColumnName("CanBoID");
        builder.Property(e => e.ChucVuId).HasColumnName("ChucVuID");
        builder.Property(e => e.DonViId).HasColumnName("DonViID");

    }
}