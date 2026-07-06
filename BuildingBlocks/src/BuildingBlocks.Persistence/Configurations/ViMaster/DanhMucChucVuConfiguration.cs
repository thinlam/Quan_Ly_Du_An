using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Persistence.Configurations.ViMaster;

public class DanhMucChucVuConfiguration : AggregateRootConfiguration<DmChucVu>
{
    public override void Configure(EntityTypeBuilder<DmChucVu> builder)
    {
        builder.HasKey(x => x.Id).HasName("PK__DmChucVu__CA9BC5E270CE69C2");
        builder.ToTable("DM_CHUCVU", "dbo", t => t.ExcludeFromMigrations());

        builder.Property(e => e.Id).HasColumnName("ChucVuID");
        builder.Property(e => e.Ten).HasColumnName("TenChucVu").HasMaxLength(500);
        builder.Property(e => e.Used).HasColumnName("Used");
    }
}