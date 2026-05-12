using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.ViMaster;

namespace QLDA.Persistence.Configurations;

public class UserMasterConfiguration : AggregateRootConfiguration<UserMaster> {
    public override void Configure(EntityTypeBuilder<UserMaster> builder) {

        builder.HasKey(e => e.Id).HasName("PK__USER_MAS__CA9BC5E270CE69C2");

        builder.ToTable("USER_MASTER");

        builder.HasIndex(e => new { DonViId = e.DonViId, e.UserPortalId, e.Used }, "IDX_USER_MASTER_01");

        builder.HasIndex(e => e.UserPortalId, "IDX_USER_MASTER_02");

        builder.HasIndex(e => new { PhongBanId = e.PhongBanId, DonViId = e.DonViId, e.UserPortalId }, "IDX_USER_MASTER_03");

        builder.HasIndex(e => new { DonViId = e.DonViId, e.UserPortalId }, "IDX_USER_MASTER_04");

        builder.HasIndex(e => e.UserPortalId, "IDX_USER_MASTER_05");

        builder.HasIndex(e => e.DonViId, "IDX_USER_MASTER_06");

        builder.HasIndex(e => new { DonViId = e.DonViId, e.UserPortalId }, "IDX_USER_MASTER_07");

        builder.HasIndex(e => e.Used, "IX_USER_MASTER_122_121");

        builder.HasIndex(e => new { e.CanBoId, e.Used }, "IX_USER_MASTER_CanBoID_Used");
        builder.Property(e => e.CanBoId).HasColumnName("CanBoID");
        builder.Property(e => e.DonViId).HasColumnName("DonViID");
        builder.Property(e => e.HoTen).HasMaxLength(50);
        builder.Property(e => e.PhongBanId).HasColumnName("PhongBanID");
        builder.Property(e => e.Id).HasColumnName("User_MasterID");
        builder.Property(e => e.UserName).HasMaxLength(50);
        builder.Property(e => e.UserPortalId).HasColumnName("User_PortalID");
    }
}
