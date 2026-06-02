using BuildingBlocks.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace QLDA.Persistence.Configurations;

public class CanBoTrienKhaiHangMucConfiguration    : AggregateRootConfiguration<CanBoTrienKhaiHangMuc>
{
    public override void Configure(EntityTypeBuilder<CanBoTrienKhaiHangMuc> builder)
    {
        builder.ToTable(nameof(CanBoTrienKhaiHangMuc));

        builder.HasKey(e => new { e.KeHoachId, e.CanBoId });

        builder.HasOne(e => e.KeHoachTrienKhai)
            .WithMany(e => e.CanBoTrienKhais)
            .HasForeignKey(e => e.KeHoachId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}