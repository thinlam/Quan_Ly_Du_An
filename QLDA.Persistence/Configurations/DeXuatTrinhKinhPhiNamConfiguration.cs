using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class DeXuatTrinhKinhPhiNamConfiguration : AggregateRootConfiguration<DeXuatTrinhKinhPhiNam>
{
    public override void Configure(EntityTypeBuilder<DeXuatTrinhKinhPhiNam> builder)
    {

        builder.ToTable(nameof(DeXuatTrinhKinhPhiNam));

        builder.HasKey(e => new { e.LeftId, e.RightId });

        builder.Property(e => e.LeftId).HasColumnName("DeXuatKinhPhiNamId");
        builder.Property(e => e.RightId).HasColumnName("DeXuatNhuCauKinhPhiId");

        builder.HasOne(e => e.DeXuatNhuCauKinhPhi) 
            .WithMany(e => e.DeXuats) 
            .HasForeignKey(e => e.LeftId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}