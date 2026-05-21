using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities;

namespace QLDA.Persistence.Configurations;

public class NoiDungDaKySoConfiguration : AggregateRootConfiguration<NoiDungDaKySo> {
    public override void Configure(EntityTypeBuilder<NoiDungDaKySo> builder) {
        builder.ToTable("NoiDungDaKySo");
        builder.ConfigureForBase();

        builder.Property(e => e.FileName).HasMaxLength(500);
        builder.Property(e => e.FileOrginal).HasMaxLength(500);
        builder.Property(e => e.GroupId).HasMaxLength(100);
        builder.Property(e => e.GroupName).HasMaxLength(200);

        builder.HasOne(e => e.TepDinhKem)
            .WithMany()
            .HasForeignKey(e => e.TepDinhKemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
