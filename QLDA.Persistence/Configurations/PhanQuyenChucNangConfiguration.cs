using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QLDA.Domain.Entities.DanhMuc;

namespace QLDA.Persistence.Configurations;

public class PhanQuyenChucNangConfiguration : AggregateRootConfiguration<PhanQuyenChucNang> {
    public override void Configure(EntityTypeBuilder<PhanQuyenChucNang> builder) {
        builder.ToTable(nameof(PhanQuyenChucNang));

        builder.Property(e => e.MaChucNang)
           .HasColumnType("varchar(100)")
            .IsRequired();

        builder.Property(e => e.ChucNang)
           .HasColumnType("nvarchar(500)")
            .IsRequired();

    }
}